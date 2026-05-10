using FluentAssertions;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Errors;

namespace TestWorkTogetherly.Domain;

public class BookingTests
{
    private static readonly DateTime SlotStart = new(2030, 6, 1, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime SlotEnd = new(2030, 6, 1, 18, 0, 0, DateTimeKind.Utc);

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WhenArrivalTimeWithinSlot_ReturnsBooking()
    {
        var result = Booking.Create(1, Guid.NewGuid(), new TimeOnly(9, 0), SlotStart, SlotEnd);

        result.IsError.Should().BeFalse();
        result.Value.ArrivalTime.Should().Be(new TimeOnly(9, 0));
    }

    [Fact]
    public void Create_WhenArrivalTimeAtSlotStart_ReturnsBooking()
    {
        var result = Booking.Create(1, Guid.NewGuid(), new TimeOnly(8, 0), SlotStart, SlotEnd);

        result.IsError.Should().BeFalse();
    }

    [Fact]
    public void Create_WhenArrivalTimeAtSlotEnd_ReturnsBooking()
    {
        var result = Booking.Create(1, Guid.NewGuid(), new TimeOnly(18, 0), SlotStart, SlotEnd);

        result.IsError.Should().BeFalse();
    }

    [Fact]
    public void Create_WhenArrivalTimeBeforeSlotStart_ReturnsArrivalTimeOutOfRange()
    {
        var result = Booking.Create(1, Guid.NewGuid(), new TimeOnly(7, 59), SlotStart, SlotEnd);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BookingErrors.ArrivalTimeOutOfRange);
    }

    [Fact]
    public void Create_WhenArrivalTimeAfterSlotEnd_ReturnsArrivalTimeOutOfRange()
    {
        var result = Booking.Create(1, Guid.NewGuid(), new TimeOnly(18, 1), SlotStart, SlotEnd);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BookingErrors.ArrivalTimeOutOfRange);
    }

    // ── Cancel ────────────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_WhenNotCancelled_SetsIsCancelled()
    {
        var booking = Booking.Create(1, Guid.NewGuid(), new TimeOnly(9, 0), SlotStart, SlotEnd).Value;

        var result = booking.Cancel();

        result.IsError.Should().BeFalse();
        booking.IsCancelled.Should().BeTrue();
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ReturnsAlreadyCancelled()
    {
        var booking = Booking.Create(1, Guid.NewGuid(), new TimeOnly(9, 0), SlotStart, SlotEnd).Value;
        booking.Cancel();

        var result = booking.Cancel();

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BookingErrors.AlreadyCancelled);
    }

    // ── UpdateArrivalTime ─────────────────────────────────────────────────────

    [Fact]
    public void UpdateArrivalTime_WhenValidTime_UpdatesTime()
    {
        var booking = Booking.Create(1, Guid.NewGuid(), new TimeOnly(9, 0), SlotStart, SlotEnd).Value;

        var result = booking.UpdateArrivalTime(new TimeOnly(10, 0), SlotStart, SlotEnd);

        result.IsError.Should().BeFalse();
        booking.ArrivalTime.Should().Be(new TimeOnly(10, 0));
    }

    [Fact]
    public void UpdateArrivalTime_WhenOutOfRange_ReturnsArrivalTimeOutOfRange()
    {
        var booking = Booking.Create(1, Guid.NewGuid(), new TimeOnly(9, 0), SlotStart, SlotEnd).Value;

        var result = booking.UpdateArrivalTime(new TimeOnly(19, 0), SlotStart, SlotEnd);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BookingErrors.ArrivalTimeOutOfRange);
    }

    [Fact]
    public void UpdateArrivalTime_WhenAlreadyCancelled_ReturnsAlreadyCancelled()
    {
        var booking = Booking.Create(1, Guid.NewGuid(), new TimeOnly(9, 0), SlotStart, SlotEnd).Value;
        booking.Cancel();

        var result = booking.UpdateArrivalTime(new TimeOnly(10, 0), SlotStart, SlotEnd);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BookingErrors.AlreadyCancelled);
    }
}
