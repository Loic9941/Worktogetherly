using ErrorOr;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;
using WorkTogetherly.Application.Bookings.CancelBooking;
using WorkTogetherly.Application.Bookings.Common;
using WorkTogetherly.Application.Bookings.CreateBooking;
using WorkTogetherly.Application.Bookings.GetUserBookings;
using WorkTogetherly.Application.Bookings.UpdateBookingArrivalTime;
using WorkTogetherly.Presentation.Controllers.Booking;
using WorkTogetherly.Presentation.Models.Booking;

namespace TestWorkTogetherly.Presentation.Booking;

public class BookingControllerTests
{
    private static readonly BookingResult SampleBookingResult = new(
        Id: 1,
        SlotId: 10,
        SlotStart: new DateTime(2025, 6, 1, 9, 0, 0),
        SlotEnd: new DateTime(2025, 6, 1, 17, 0, 0),
        ArrivalTime: new TimeOnly(10, 0),
        IsCancelled: false,
        Workspace: new WorkspaceSummary(1, "Espace A", "1 rue test"),
        HasReview: false);

    private static BookingController CreateController(IMediator mediator, Guid? userId = null)
    {
        var controller = new BookingController(mediator);
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
    public async Task Create_WhenHandlerSucceeds_ReturnsOkWithId()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateBookingCommand>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<int>)42);

        var controller = CreateController(mediator, userId);
        var request = new CreateBookingRequest(SlotId: 10, ArrivalTime: new TimeOnly(10, 0), MaterialIds: null);

        var result = await controller.Create(request);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new { Id = 42 });
    }

    [Fact]
    public async Task Create_WhenSlotFullyBooked_Returns409()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateBookingCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.Conflict("Slot.FullyBooked", "Créneau complet"));

        var controller = CreateController(mediator, userId);
        var request = new CreateBookingRequest(SlotId: 10, ArrivalTime: new TimeOnly(10, 0), MaterialIds: null);

        var result = await controller.Create(request);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Create_WhenValidationError_Returns400WithModelState()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateBookingCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.Validation("ArrivalTime", "Heure d'arrivée hors plage"));

        var controller = CreateController(mediator, userId);
        var request = new CreateBookingRequest(SlotId: 10, ArrivalTime: new TimeOnly(23, 0), MaterialIds: null);

        var result = await controller.Create(request);

        result.Should().BeOfType<ObjectResult>()
            .Which.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    [Fact]
    public async Task Create_MapsRequestFieldsToCommand()
    {
        var userId = Guid.NewGuid();
        var arrivalTime = new TimeOnly(10, 30);
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateBookingCommand>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<int>)1);

        var controller = CreateController(mediator, userId);
        await controller.Create(new CreateBookingRequest(SlotId: 7, ArrivalTime: arrivalTime, MaterialIds: null));

        await mediator.Received(1).Send(
            Arg.Is<CreateBookingCommand>(c => c.SlotId == 7 && c.UserId == userId && c.ArrivalTime == arrivalTime),
            Arg.Any<CancellationToken>());
    }

    // ── CancelBooking ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelBooking_WhenHandlerSucceeds_Returns204()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CancelBookingCommand>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<BookingResult>)SampleBookingResult);

        var controller = CreateController(mediator, userId);

        var result = await controller.CancelBooking(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task CancelBooking_WhenNotFound_Returns404()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CancelBookingCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.NotFound("Booking.NotFound", "Réservation introuvable"));

        var controller = CreateController(mediator, userId);

        var result = await controller.CancelBooking(99);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task CancelBooking_WhenUnauthorized_Returns403()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CancelBookingCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.Forbidden("Booking.Unauthorized", "Accès refusé"));

        var controller = CreateController(mediator, userId);

        var result = await controller.CancelBooking(1);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task CancelBooking_MapsIdAndUserIdToCommand()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CancelBookingCommand>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<BookingResult>)SampleBookingResult);

        var controller = CreateController(mediator, userId);
        await controller.CancelBooking(5);

        await mediator.Received(1).Send(
            Arg.Is<CancelBookingCommand>(c => c.BookingId == 5 && c.UserId == userId),
            Arg.Any<CancellationToken>());
    }

    // ── UpdateArrivalTime ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateArrivalTime_WhenHandlerSucceeds_ReturnsOkWithResult()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateBookingArrivalTimeCommand>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<BookingResult>)SampleBookingResult);

        var controller = CreateController(mediator, userId);
        var request = new UpdateArrivalTimeRequest(new TimeOnly(11, 0));

        var result = await controller.UpdateArrivalTime(1, request);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(SampleBookingResult);
    }

    [Fact]
    public async Task UpdateArrivalTime_WhenArrivalTimePassed_Returns409()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateBookingArrivalTimeCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.Conflict("Booking.ArrivalTimePassed", "Heure dépassée"));

        var controller = CreateController(mediator, userId);

        var result = await controller.UpdateArrivalTime(1, new UpdateArrivalTimeRequest(new TimeOnly(8, 0)));

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task UpdateArrivalTime_MapsFieldsToCommand()
    {
        var userId = Guid.NewGuid();
        var newTime = new TimeOnly(14, 0);
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateBookingArrivalTimeCommand>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<BookingResult>)SampleBookingResult);

        var controller = CreateController(mediator, userId);
        await controller.UpdateArrivalTime(3, new UpdateArrivalTimeRequest(newTime));

        await mediator.Received(1).Send(
            Arg.Is<UpdateBookingArrivalTimeCommand>(c => c.BookingId == 3 && c.UserId == userId && c.NewArrivalTime == newTime),
            Arg.Any<CancellationToken>());
    }

    // ── GetMyBookings ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMyBookings_WhenHandlerSucceeds_ReturnsOkWithList()
    {
        var userId = Guid.NewGuid();
        var bookings = new List<BookingResult> { SampleBookingResult };
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetUserBookingsQuery>(), Arg.Any<CancellationToken>())
                .Returns(ErrorOrFactory.From<IReadOnlyList<BookingResult>>(bookings));

        var controller = CreateController(mediator, userId);

        var result = await controller.GetMyBookings(null, null);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(bookings);
    }

    [Fact]
    public async Task GetMyBookings_WhenNoDatesProvided_UsesDefaultRange()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetUserBookingsQuery>(), Arg.Any<CancellationToken>())
                .Returns(ErrorOrFactory.From<IReadOnlyList<BookingResult>>(new List<BookingResult>()));

        var controller = CreateController(mediator, userId);
        var before = DateTime.UtcNow.Date;

        await controller.GetMyBookings(null, null);

        await mediator.Received(1).Send(
            Arg.Is<GetUserBookingsQuery>(q =>
                q.UserId == userId &&
                q.From >= before &&
                q.To >= q.From.AddDays(6)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMyBookings_WhenExplicitDatesProvided_UsesThem()
    {
        var userId = Guid.NewGuid();
        var from = new DateTime(2025, 7, 1);
        var to = new DateTime(2025, 7, 31);
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetUserBookingsQuery>(), Arg.Any<CancellationToken>())
                .Returns(ErrorOrFactory.From<IReadOnlyList<BookingResult>>(new List<BookingResult>()));

        var controller = CreateController(mediator, userId);
        await controller.GetMyBookings(from, to);

        await mediator.Received(1).Send(
            Arg.Is<GetUserBookingsQuery>(q => q.From == from && q.To == to),
            Arg.Any<CancellationToken>());
    }
}
