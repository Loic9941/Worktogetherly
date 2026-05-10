using ErrorOr;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;
using WorkTogetherly.Application.Workspaces.Common;
using WorkTogetherly.Application.Workspaces.CreateWorkspace;
using WorkTogetherly.Application.Workspaces.GetMyWorkspace;
using WorkTogetherly.Application.Workspaces.Search;
using WorkTogetherly.Application.Workspaces.UpdateWorkspace;
using WorkTogetherly.Presentation.Controllers.Workspace;
using WorkTogetherly.Presentation.Models.Workspace;

namespace TestWorkTogetherly.Presentation.Workspace;

public class WorkspaceControllerTests
{
    private static readonly WorkspaceResult SampleWorkspaceResult = new(
        Id: 1,
        UserId: Guid.NewGuid(),
        Name: "Mon espace",
        Description: "Description",
        Address: "1 rue de la Poste, Bruxelles",
        Latitude: 48.869,
        Longitude: 48.869,
        Capacity: 10,
        IsActive: true,
        CreatedAt: DateTime.UtcNow,
        Materials: [],
        Rules: [],
        PhotoPath: null
    );

    private static readonly CreateWorkspaceRequest SampleCreateRequest = new(
        Name: "Mon espace",
        Description: "Description",
        Address: "1 rue de la Poste, Bruxelles",
        Latitude: 48.869,
        Longitude: 48.869,
        Capacity: 10,
        IsActive: true,
        Materials: [],
        RuleIds: []);

    private static readonly UpdateWorkspaceRequest SampleUpdateRequest = new(
        Name: "Mon espace MàJ",
        Description: "Description MàJ",
        Address: "1 rue de la Poste, Bruxelles",
        Latitude: 48.869,
        Longitude: 48.869,
        Capacity: 15,
        IsActive: true,
        Materials: [],
        RuleIds: []);

    private static WorkspaceController CreateController(IMediator mediator, Guid? userId = null)
    {
        var controller = new WorkspaceController(mediator);
        var claims = userId.HasValue
            ? new[] { new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()) }
            : Array.Empty<Claim>();

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims))
            }
        };
        return controller;
    }

    // ── Search ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Search_WhenHandlerSucceeds_ReturnsOkWithResults()
    {
        var searchResults = new List<WorkspaceSearchResult>
        {
            new(1, "Espace A", "Desc", 48.869, 2.331, 10, null, 10, [], [])
        };

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<SearchWorkspacesQuery>(), Arg.Any<CancellationToken>())
                .Returns(searchResults);

        var controller = CreateController(mediator, Guid.NewGuid());
        var date = DateOnly.FromDateTime(DateTime.Today);

        var result = await controller.Search(48.869, 2.331, date, 20);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(searchResults);
    }

    [Fact]
    public async Task Search_WhenHandlerReturnsValidationError_Returns400WithModelState()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<SearchWorkspacesQuery>(), Arg.Any<CancellationToken>())
                .Returns(Error.Validation("Latitude", "Latitude invalide"));

        var controller = CreateController(mediator, Guid.NewGuid());

        var result = await controller.Search(999, 2.331, DateOnly.FromDateTime(DateTime.Today));

        result.Should().BeOfType<ObjectResult>()
            .Which.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    // ── GetMyWorkspace ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMyWorkspace_WhenWorkspaceExists_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetMyWorkspaceQuery>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<WorkspaceResult?>)SampleWorkspaceResult);

        var controller = CreateController(mediator, userId);

        var result = await controller.GetMyWorkspace();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(SampleWorkspaceResult);
    }

    [Fact]
    public async Task GetMyWorkspace_WhenNoWorkspace_Returns404()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetMyWorkspaceQuery>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<WorkspaceResult?>)(WorkspaceResult?)null);

        var controller = CreateController(mediator, userId);

        var result = await controller.GetMyWorkspace();

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task WorkspaceController_HasClassLevelAuthorizeAttribute()
    {
        typeof(WorkspaceController).Should().BeDecoratedWith<AuthorizeAttribute>();
        await Task.CompletedTask;
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_WhenHandlerSucceeds_Returns201()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateWorkspaceCommand>(), Arg.Any<CancellationToken>())
                .Returns(SampleWorkspaceResult);

        var controller = CreateController(mediator, userId);

        var result = await controller.Create(SampleCreateRequest);

        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.Value.Should().Be(SampleWorkspaceResult);
    }

    [Fact]
    public async Task Create_WhenHandlerReturnsValidationError_Returns400WithModelState()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateWorkspaceCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.Validation("Name", "Nom requis"));

        var controller = CreateController(mediator, userId);

        var result = await controller.Create(SampleCreateRequest);

        result.Should().BeOfType<ObjectResult>()
            .Which.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    [Fact]
    public async Task Create_MapsRequestFieldsToCommand()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateWorkspaceCommand>(), Arg.Any<CancellationToken>())
                .Returns(SampleWorkspaceResult);

        var controller = CreateController(mediator, userId);

        await controller.Create(SampleCreateRequest);

        await mediator.Received(1).Send(
            Arg.Is<CreateWorkspaceCommand>(c =>
                c.UserId == userId &&
                c.Name == SampleCreateRequest.Name &&
                c.Capacity == SampleCreateRequest.Capacity),
            Arg.Any<CancellationToken>());
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_WhenHandlerSucceeds_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateWorkspaceCommand>(), Arg.Any<CancellationToken>())
                .Returns(SampleWorkspaceResult);

        var controller = CreateController(mediator, userId);

        var result = await controller.Update(1, SampleUpdateRequest);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(SampleWorkspaceResult);
    }

    [Fact]
    public async Task Update_WhenUnauthorizedOwner_Returns403()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateWorkspaceCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.Forbidden("Workspace.Unauthorized", "Accès refusé"));

        var controller = CreateController(mediator, userId);

        var result = await controller.Update(1, SampleUpdateRequest);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task Update_WhenWorkspaceNotFound_Returns404()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateWorkspaceCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.NotFound("Workspace.NotFound", "Espace introuvable"));

        var controller = CreateController(mediator, userId);

        var result = await controller.Update(99, SampleUpdateRequest);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Update_MapsIdAndUserIdToCommand()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateWorkspaceCommand>(), Arg.Any<CancellationToken>())
                .Returns(SampleWorkspaceResult);

        var controller = CreateController(mediator, userId);

        await controller.Update(42, SampleUpdateRequest);

        await mediator.Received(1).Send(
            Arg.Is<UpdateWorkspaceCommand>(c => c.WorkspaceId == 42 && c.UserId == userId),
            Arg.Any<CancellationToken>());
    }
}
