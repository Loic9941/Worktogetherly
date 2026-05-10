using FluentAssertions;
using NSubstitute;
using TestWorkTogetherly.Helpers;
using WorkTogetherly.Application.Bookings.UpdateBookingArrivalTime;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Domain.Interfaces;
using AppBookingErrors = WorkTogetherly.Application.Errors.BookingErrors;
using DomainBookingErrors = WorkTogetherly.Domain.Errors.BookingErrors;

namespace TestWorkTogetherly.Application.Booking;

public class UpdateBookingArrivalTimeHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime FutureStart = DateTime.UtcNow.AddDays(1).Date.AddHours(8);
    private static readonly DateTime FutureEnd = FutureStart.AddHours(10);

    private readonly IBookingRepository _bookingRepo = Substitute.For<IBookingRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly UpdateBookingArrivalTimeHandler _handler;

    public UpdateBookingArrivalTimeHandlerTests()
    {
        _clock.UtcNow.Returns(new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc));
        _handler = new UpdateBookingArrivalTimeHandler(_bookingRepo, _clock);
    }

    [Fact]
    public async Task Handle_WhenBookingNotFound_ReturnsNotFound()
    {
        _bookingRepo.GetByIdAsync(1).Returns((WorkTogetherly.Domain.Entities.Booking?)null);

        var result = await _handler.Handle(new UpdateBookingArrivalTimeCommand(1, UserId, new TimeOnly(10, 0)), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainBookingErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenUserNotOwner_ReturnsUnauthorized()
    {
        var booking = EntityFactory.MakeBooking(userId: Guid.NewGuid());
        _bookingRepo.GetByIdAsync(1).Returns(booking);

        var result = await _handler.Handle(new UpdateBookingArrivalTimeCommand(1, UserId, new TimeOnly(10, 0)), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppBookingErrors.Unauthorized.Code);
    }

    [Fact]
    public async Task Handle_WhenArrivalTimePassed_ReturnsArrivalTimePassed()
    {
        var pastSlot = EntityFactory.MakeSlot(
            start: DateTime.UtcNow.AddHours(-3),
            end: DateTime.UtcNow.AddHours(-1));
        var booking = EntityFactory.MakeBooking(
            userId: UserId,
            arrivalTime: new TimeOnly(DateTime.UtcNow.AddHours(-2).Hour, 0),
            slot: pastSlot);
        _bookingRepo.GetByIdAsync(1).Returns(booking);

        var result = await _handler.Handle(new UpdateBookingArrivalTimeCommand(1, UserId, new TimeOnly(10, 0)), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppBookingErrors.ArrivalTimePassed.Code);
    }

    [Fact]
    public async Task Handle_WhenNewArrivalTimeOutOfSlotRange_ReturnsArrivalTimeOutOfRange()
    {
        var slot = EntityFactory.MakeSlot(start: FutureStart, end: FutureEnd);
        var booking = EntityFactory.MakeBooking(userId: UserId, arrivalTime: new TimeOnly(9, 0), slot: slot);
        _bookingRepo.GetByIdAsync(1).Returns(booking);

        var result = await _handler.Handle(new UpdateBookingArrivalTimeCommand(1, UserId, new TimeOnly(23, 0)), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainBookingErrors.ArrivalTimeOutOfRange.Code);
    }

    [Fact]
    public async Task Handle_WhenValid_UpdatesArrivalTimeAndSaves()
    {
        var slot = EntityFactory.MakeSlot(start: FutureStart, end: FutureEnd);
        var booking = EntityFactory.MakeBooking(userId: UserId, arrivalTime: new TimeOnly(9, 0), slot: slot);
        _bookingRepo.GetByIdAsync(1).Returns(booking);

        var result = await _handler.Handle(new UpdateBookingArrivalTimeCommand(1, UserId, new TimeOnly(10, 0)), default);

        result.IsError.Should().BeFalse();
        booking.ArrivalTime.Should().Be(new TimeOnly(10, 0));
        await _bookingRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
