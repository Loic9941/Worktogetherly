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

    // ── HasArrivalTimePassed ──────────────────────────────────────────────────

    [Fact]
    public void HasArrivalTimePassed_WhenArrivalInFuture_ReturnsFalse()
    {
        var start = new DateTime(2030, 6, 1, 8, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2030, 6, 1, 18, 0, 0, DateTimeKind.Utc);
        var booking = Booking.Create(1, Guid.NewGuid(), new TimeOnly(10, 0), start, end).Value;
        // Need to set Slot navigation for HasArrivalTimePassed
        typeof(Booking).GetProperty("Slot")!.SetValue(booking, CreateSlotForBooking(start, end));

        var result = booking.HasArrivalTimePassed(new DateTime(2030, 6, 1, 9, 0, 0, DateTimeKind.Utc));

        result.Should().BeFalse();
    }

    [Fact]
    public void HasArrivalTimePassed_WhenArrivalInPast_ReturnsTrue()
    {
        var start = new DateTime(2030, 6, 1, 8, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2030, 6, 1, 18, 0, 0, DateTimeKind.Utc);
        var booking = Booking.Create(1, Guid.NewGuid(), new TimeOnly(9, 0), start, end).Value;
        typeof(Booking).GetProperty("Slot")!.SetValue(booking, CreateSlotForBooking(start, end));

        var result = booking.HasArrivalTimePassed(new DateTime(2030, 6, 1, 10, 0, 0, DateTimeKind.Utc));

        result.Should().BeTrue();
    }

    // ── IsSlotEnded ───────────────────────────────────────────────────────────

    [Fact]
    public void IsSlotEnded_WhenSlotNotYetEnded_ReturnsFalse()
    {
        var start = new DateTime(2030, 6, 1, 8, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2030, 6, 1, 18, 0, 0, DateTimeKind.Utc);
        var booking = Booking.Create(1, Guid.NewGuid(), new TimeOnly(9, 0), start, end).Value;
        typeof(Booking).GetProperty("Slot")!.SetValue(booking, CreateSlotForBooking(start, end));

        var result = booking.IsSlotEnded(new DateTime(2030, 6, 1, 12, 0, 0, DateTimeKind.Utc));

        result.Should().BeFalse();
    }

    [Fact]
    public void IsSlotEnded_WhenSlotEnded_ReturnsTrue()
    {
        var start = new DateTime(2030, 6, 1, 8, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2030, 6, 1, 18, 0, 0, DateTimeKind.Utc);
        var booking = Booking.Create(1, Guid.NewGuid(), new TimeOnly(9, 0), start, end).Value;
        typeof(Booking).GetProperty("Slot")!.SetValue(booking, CreateSlotForBooking(start, end));

        var result = booking.IsSlotEnded(new DateTime(2030, 6, 1, 19, 0, 0, DateTimeKind.Utc));

        result.Should().BeTrue();
    }

    // ── AddMaterials ──────────────────────────────────────────────────────────

    [Fact]
    public void AddMaterials_WhenCalled_PopulatesBookingMaterials()
    {
        var booking = Booking.Create(1, Guid.NewGuid(), new TimeOnly(9, 0), SlotStart, SlotEnd).Value;

        booking.AddMaterials([1, 2, 3]);

        booking.BookingMaterials.Should().HaveCount(3);
        booking.BookingMaterials.Should().Contain(bm => bm.MaterialId == 1);
        booking.BookingMaterials.Should().Contain(bm => bm.MaterialId == 3);
    }

    [Fact]
    public void AddMaterials_WhenEmptyList_LeavesCollectionUnchanged()
    {
        var booking = Booking.Create(1, Guid.NewGuid(), new TimeOnly(9, 0), SlotStart, SlotEnd).Value;

        booking.AddMaterials([]);

        booking.BookingMaterials.Should().BeEmpty();
    }

    private static Slot CreateSlotForBooking(DateTime start, DateTime end)
    {
        var slot = (Slot)Activator.CreateInstance(typeof(Slot), nonPublic: true)!;
        typeof(Slot).GetProperty("StartDateTime")!.SetValue(slot, start);
        typeof(Slot).GetProperty("EndDateTime")!.SetValue(slot, end);
        return slot;
    }
}
