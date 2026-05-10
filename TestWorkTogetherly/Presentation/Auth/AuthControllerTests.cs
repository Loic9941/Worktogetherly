using ErrorOr;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;
using WorkTogetherly.Application.Users.GetCurrentUser;
using WorkTogetherly.Application.Users.Login;
using WorkTogetherly.Application.Users.Logout;
using WorkTogetherly.Application.Users.RefreshToken;
using WorkTogetherly.Application.Users.Register;
using WorkTogetherly.Application.Users.Shared;
using WorkTogetherly.Presentation.Controllers.Auth;
using WorkTogetherly.Presentation.Models.Auth;

namespace TestWorkTogetherly.Presentation.Auth;

public class AuthControllerTests
{
    private static readonly AuthResult SampleAuthResult = new(
        AccessToken: "access-token",
        RefreshToken: "refresh-token",
        ExpiresAt: DateTime.UtcNow.AddHours(1));
    
    // Helper method to create a controller with a mocked HttpContext
    private static AuthController CreateController(IMediator mediator, Guid? userId = null, string? cookieRefreshToken = null)
    {
        var controller = new AuthController(mediator);
        var claims = userId.HasValue
            ? new[] { new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()) }
            : Array.Empty<Claim>();

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims))
        };

        if (cookieRefreshToken is not null)
            httpContext.Request.Headers["Cookie"] = $"refresh_token={cookieRefreshToken}";

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    // ── Register ──────────────────────────────────────────────────────────────

    // Tests for the Register endpoint covering success, conflict
    [Fact]
    public async Task Register_WhenHandlerSucceeds_ReturnsOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RegisterCommand>(), Arg.Any<CancellationToken>())
                .Returns(SampleAuthResult);

        var controller = CreateController(mediator);
        var request = new RegisterRequest("user@test.com", "Password1!", "Jane", "Doe");

        var result = await controller.Register(request);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(SampleAuthResult);
    }

    // Tests that a conflict error from the handler results in a 409 response
    [Fact]
    public async Task Register_WhenHandlerReturnsConflict_Returns409()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RegisterCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.Conflict("User.EmailAlreadyExists", "Email déjà utilisé"));

        var controller = CreateController(mediator);
        var request = new RegisterRequest("user@test.com", "Password1!", "Jane", "Doe");

        var result = await controller.Register(request);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    // Tests that a validation error from the handler results in a 400 response with model state details
    [Fact]
    public async Task Register_WhenHandlerReturnsValidationError_Returns400WithModelState()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RegisterCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.Validation("Email", "Email invalide"));

        var controller = CreateController(mediator);
        var request = new RegisterRequest("bad", "Password1!", "Jane", "Doe");

        var result = await controller.Register(request);

        result.Should().BeOfType<ObjectResult>()
            .Which.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    // Tests for the Login endpoint covering success
    [Fact]
    public async Task Login_WhenHandlerSucceeds_ReturnsOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<LoginCommand>(), Arg.Any<CancellationToken>())
                .Returns(SampleAuthResult);

        var controller = CreateController(mediator);
        var request = new LoginRequest("user@test.com", "Password1!");

        var result = await controller.Login(request);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(SampleAuthResult);
    }

    //Test that invalid credentials result in a 401 response
    [Fact]
    public async Task Login_WhenInvalidCredentials_Returns401()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<LoginCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.Unauthorized("User.InvalidCredentials", "Identifiants invalides"));

        var controller = CreateController(mediator);
        var request = new LoginRequest("user@test.com", "wrong");

        var result = await controller.Login(request);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    // ── Refresh ───────────────────────────────────────────────────────────────

    // Tests for the Refresh endpoint covering success
    [Fact]
    public async Task Refresh_WhenCookieHasToken_ReturnsOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RefreshTokenCommand>(), Arg.Any<CancellationToken>())
                .Returns(SampleAuthResult);

        var controller = CreateController(mediator, cookieRefreshToken: "my-refresh-token");

        var result = await controller.Refresh();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(SampleAuthResult);

        await mediator.Received(1).Send(
            Arg.Is<RefreshTokenCommand>(c => c.RefreshToken == "my-refresh-token"),
            Arg.Any<CancellationToken>());
    }

    //Test that if no token is provided in cookie or body, the controller returns a 400 Bad Request
    [Fact]
    public async Task Refresh_WhenNoCookieAndNoBody_Returns400()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);

        var result = await controller.Refresh();

        result.Should().BeOfType<BadRequestObjectResult>();
        await mediator.DidNotReceive().Send(Arg.Any<RefreshTokenCommand>(), Arg.Any<CancellationToken>());
    }

    // Test that if the handler returns an unauthorized error (e.g. invalid/expired token), the controller returns a 401 Unauthorized response
    [Fact]
    public async Task Refresh_WhenHandlerReturnsError_ReturnsErrorStatus()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RefreshTokenCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.Unauthorized("User.InvalidRefreshToken", "Token invalide"));

        var controller = CreateController(mediator, cookieRefreshToken: "expired-token");

        var result = await controller.Refresh();

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    // Tests for the Logout endpoint covering the case where a refresh token is provided in the cookie
    [Fact]
    public async Task Logout_WhenCookieHasToken_SendsCommandAndReturns204()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<LogoutCommand>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<bool>)true);

        var userId = Guid.NewGuid();
        var controller = CreateController(mediator, userId, cookieRefreshToken: "my-refresh-token");

        var result = await controller.Logout();

        result.Should().BeOfType<NoContentResult>();
        await mediator.Received(1).Send(
            Arg.Is<LogoutCommand>(c => c.RefreshToken == "my-refresh-token"),
            Arg.Any<CancellationToken>());
    }

    // Test that if no refresh token is provided in the cookie, the controller returns a 204 No Content response without sending any command to the mediator
    [Fact]
    public async Task Logout_WhenNoToken_Returns204WithoutSendingCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var userId = Guid.NewGuid();
        var controller = CreateController(mediator, userId);

        var result = await controller.Logout();

        result.Should().BeOfType<NoContentResult>();
        await mediator.DidNotReceive().Send(Arg.Any<LogoutCommand>(), Arg.Any<CancellationToken>());
    }

    // ── GetCurrentUser ────────────────────────────────────────────────────────

    // Tests for the GetCurrentUser endpoint covering the case where a valid user ID claim is present
    [Fact]
    public async Task GetCurrentUser_WhenClaimPresent_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var currentUser = new CurrentUserResult(userId, "user@test.com", "Jane", "Doe");

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetCurrentUserQuery>(), Arg.Any<CancellationToken>())
                .Returns(currentUser);

        var controller = CreateController(mediator, userId);

        var result = await controller.GetCurrentUser();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(currentUser);
    }

    // 401 enforcement is handled by [Authorize] on the method — verified here via reflection
    [Fact]
    public async Task GetCurrentUser_HasAuthorizeAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.GetCurrentUser));
        method.Should().BeDecoratedWith<AuthorizeAttribute>();
        await Task.CompletedTask;
    }

    //Test that if the handler returns a not found error (e.g. user ID from claim does not exist), the controller returns a 404 Not Found response
    [Fact]
    public async Task GetCurrentUser_WhenHandlerReturnsNotFound_Returns404()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetCurrentUserQuery>(), Arg.Any<CancellationToken>())
                .Returns(Error.NotFound("User.NotFound", "Utilisateur introuvable"));

        var controller = CreateController(mediator, userId);

        var result = await controller.GetCurrentUser();

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}
