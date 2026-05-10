using ErrorOr;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;
using WorkTogetherly.Application.Reviews.Common;
using WorkTogetherly.Application.Reviews.CreateReview;
using WorkTogetherly.Application.Reviews.GetReviewByBooking;
using WorkTogetherly.Application.Reviews.UpdateReview;
using WorkTogetherly.Presentation.Controllers.Review;
using WorkTogetherly.Presentation.Models.Review;

namespace TestWorkTogetherly.Presentation.Review;

public class ReviewControllerTests
{
    private static readonly ReviewResult SampleReviewResult = new(
        Id: 1,
        BookingId: 5,
        WorkspaceId: 10,
        Rating: 4,
        Comment: "Très bien",
        CreatedAt: new DateTime(2025, 5, 1),
        ReviewerName: "Jane D.");

    private static ReviewController CreateController(IMediator mediator, Guid? userId = null)
    {
        var controller = new ReviewController(mediator);
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

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_WhenHandlerSucceeds_ReturnsOkWithResult()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateReviewCommand>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<ReviewResult>)SampleReviewResult);

        var controller = CreateController(mediator, userId);
        var request = new CreateReviewRequest(BookingId: 5, Rating: 4, Comment: "Très bien");

        var result = await controller.Create(request);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(SampleReviewResult);
    }

    [Fact]
    public async Task Create_WhenReviewAlreadyExists_Returns409()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateReviewCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.Conflict("Review.AlreadyExists", "Avis déjà existant"));

        var controller = CreateController(mediator, userId);
        var request = new CreateReviewRequest(BookingId: 5, Rating: 4, Comment: "Doublon");

        var result = await controller.Create(request);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Create_WhenBookingNotPast_Returns409()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateReviewCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.Conflict("Review.BookingNotPast", "Réservation pas encore passée"));

        var controller = CreateController(mediator, userId);

        var result = await controller.Create(new CreateReviewRequest(BookingId: 5, Rating: 5, Comment: "Top"));

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Create_WhenBookingNotOwned_Returns403()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateReviewCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.Forbidden("Review.BookingNotOwned", "Réservation non possédée"));

        var controller = CreateController(mediator, userId);

        var result = await controller.Create(new CreateReviewRequest(BookingId: 5, Rating: 5, Comment: "X"));

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task Create_MapsFieldsToCommand()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateReviewCommand>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<ReviewResult>)SampleReviewResult);

        var controller = CreateController(mediator, userId);
        await controller.Create(new CreateReviewRequest(BookingId: 5, Rating: 4, Comment: "Bien"));

        await mediator.Received(1).Send(
            Arg.Is<CreateReviewCommand>(c =>
                c.BookingId == 5 &&
                c.UserId == userId &&
                c.Rating == 4 &&
                c.Comment == "Bien"),
            Arg.Any<CancellationToken>());
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_WhenHandlerSucceeds_ReturnsOkWithResult()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateReviewCommand>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<ReviewResult>)SampleReviewResult);

        var controller = CreateController(mediator, userId);

        var result = await controller.Update(1, new UpdateReviewRequest(Rating: 5, Comment: "Excellent"));

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(SampleReviewResult);
    }

    [Fact]
    public async Task Update_WhenReviewNotFound_Returns404()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateReviewCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.NotFound("Review.NotFound", "Avis introuvable"));

        var controller = CreateController(mediator, userId);

        var result = await controller.Update(99, new UpdateReviewRequest(Rating: 3, Comment: "Moyen"));

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Update_WhenNotOwner_Returns403()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateReviewCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.Forbidden("Review.NotOwner", "Pas le propriétaire"));

        var controller = CreateController(mediator, userId);

        var result = await controller.Update(1, new UpdateReviewRequest(Rating: 1, Comment: "X"));

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task Update_MapsFieldsToCommand()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateReviewCommand>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<ReviewResult>)SampleReviewResult);

        var controller = CreateController(mediator, userId);
        await controller.Update(1, new UpdateReviewRequest(Rating: 5, Comment: "Super"));

        await mediator.Received(1).Send(
            Arg.Is<UpdateReviewCommand>(c =>
                c.ReviewId == 1 &&
                c.UserId == userId &&
                c.Rating == 5 &&
                c.Comment == "Super"),
            Arg.Any<CancellationToken>());
    }

    // ── GetByBooking ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByBooking_WhenReviewExists_ReturnsOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetReviewByBookingQuery>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<ReviewResult?>)SampleReviewResult);

        var controller = CreateController(mediator, Guid.NewGuid());

        var result = await controller.GetByBooking(5);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(SampleReviewResult);
    }

    [Fact]
    public async Task GetByBooking_WhenNoReview_Returns404()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetReviewByBookingQuery>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<ReviewResult?>)(ReviewResult?)null);

        var controller = CreateController(mediator, Guid.NewGuid());

        var result = await controller.GetByBooking(5);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetByBooking_MapsBookingIdToQuery()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetReviewByBookingQuery>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<ReviewResult?>)SampleReviewResult);

        var controller = CreateController(mediator, Guid.NewGuid());
        await controller.GetByBooking(7);

        await mediator.Received(1).Send(
            Arg.Is<GetReviewByBookingQuery>(q => q.BookingId == 7),
            Arg.Any<CancellationToken>());
    }
}
