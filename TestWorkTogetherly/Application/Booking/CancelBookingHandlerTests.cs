using FluentAssertions;
using NSubstitute;
using TestWorkTogetherly.Helpers;
using WorkTogetherly.Application.Bookings.CancelBooking;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Domain.Interfaces;
using AppBookingErrors = WorkTogetherly.Application.Errors.BookingErrors;
using DomainBookingErrors = WorkTogetherly.Domain.Errors.BookingErrors;

namespace TestWorkTogetherly.Application.Booking;

public class CancelBookingHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly IBookingRepository _bookingRepo = Substitute.For<IBookingRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly CancelBookingHandler _handler;

    public CancelBookingHandlerTests()
    {
        _clock.UtcNow.Returns(new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc));
        _handler = new CancelBookingHandler(_bookingRepo, _clock);
    }

    [Fact]
    public async Task Handle_WhenBookingNotFound_ReturnsNotFound()
    {
        _bookingRepo.GetByIdAsync(1).Returns((WorkTogetherly.Domain.Entities.Booking?)null);

        var result = await _handler.Handle(new CancelBookingCommand(1, UserId), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainBookingErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenUserNotOwner_ReturnsUnauthorized()
    {
        var booking = EntityFactory.MakeBooking(userId: Guid.NewGuid());
        _bookingRepo.GetByIdAsync(1).Returns(booking);

        var result = await _handler.Handle(new CancelBookingCommand(1, UserId), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppBookingErrors.Unauthorized.Code);
    }

    [Fact]
    public async Task Handle_WhenArrivalTimePassed_ReturnsArrivalTimePassed()
    {
        // Slot started in the past relative to mocked clock (2025-06-01 12:00)
        var pastSlot = EntityFactory.MakeSlot(
            start: new DateTime(2025, 6, 1, 9, 0, 0, DateTimeKind.Utc),
            end: new DateTime(2025, 6, 1, 11, 0, 0, DateTimeKind.Utc));
        var booking = EntityFactory.MakeBooking(
            userId: UserId,
            arrivalTime: new TimeOnly(10, 0),
            slot: pastSlot);
        _bookingRepo.GetByIdAsync(1).Returns(booking);

        var result = await _handler.Handle(new CancelBookingCommand(1, UserId), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppBookingErrors.ArrivalTimePassed.Code);
    }

    [Fact]
    public async Task Handle_WhenAlreadyCancelled_ReturnsAlreadyCancelled()
    {
        var futureSlot = EntityFactory.MakeSlot(
            start: DateTime.UtcNow.AddDays(1),
            end: DateTime.UtcNow.AddDays(1).AddHours(8));
        var booking = EntityFactory.MakeBooking(
            userId: UserId,
            arrivalTime: new TimeOnly(9, 0),
            slot: futureSlot,
            cancelled: true);
        _bookingRepo.GetByIdAsync(1).Returns(booking);

        var result = await _handler.Handle(new CancelBookingCommand(1, UserId), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainBookingErrors.AlreadyCancelled.Code);
    }

    [Fact]
    public async Task Handle_WhenValid_CancelsBookingAndSaves()
    {
        var futureSlot = EntityFactory.MakeSlot(
            start: DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            end: DateTime.UtcNow.AddDays(1).Date.AddHours(18));
        var booking = EntityFactory.MakeBooking(
            userId: UserId,
            arrivalTime: new TimeOnly(9, 0),
            slot: futureSlot);
        _bookingRepo.GetByIdAsync(1).Returns(booking);

        var result = await _handler.Handle(new CancelBookingCommand(1, UserId), default);

        result.IsError.Should().BeFalse();
        booking.IsCancelled.Should().BeTrue();
        await _bookingRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
