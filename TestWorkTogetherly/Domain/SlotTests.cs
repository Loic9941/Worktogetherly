using FluentAssertions;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Errors;

namespace TestWorkTogetherly.Domain;

public class SlotTests
{
    private static readonly DateTime Start = new(2030, 6, 1, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime End = new(2030, 6, 1, 18, 0, 0, DateTimeKind.Utc);

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WhenValid_ReturnsSlot()
    {
        var result = Slot.Create(1, Start, End, 5);

        result.IsError.Should().BeFalse();
        result.Value.Capacity.Should().Be(5);
        result.Value.StartDateTime.Should().Be(Start);
    }

    [Fact]
    public void Create_WhenStartEqualsEnd_ReturnsInvalidTimeRange()
    {
        var result = Slot.Create(1, Start, Start, 5);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(SlotErrors.InvalidTimeRange);
    }

    [Fact]
    public void Create_WhenStartAfterEnd_ReturnsInvalidTimeRange()
    {
        var result = Slot.Create(1, End, Start, 5);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(SlotErrors.InvalidTimeRange);
    }

    [Fact]
    public void Create_WhenCapacityZero_ReturnsInvalidCapacity()
    {
        var result = Slot.Create(1, Start, End, 0);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(SlotErrors.InvalidCapacity);
    }

    [Fact]
    public void Create_WhenCapacityNegative_ReturnsInvalidCapacity()
    {
        var result = Slot.Create(1, Start, End, -1);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(SlotErrors.InvalidCapacity);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public void Update_WhenValid_UpdatesFields()
    {
        var slot = Slot.Create(1, Start, End, 5).Value;
        var newStart = Start.AddHours(1);
        var newEnd = End.AddHours(1);

        var result = slot.Update(newStart, newEnd, 10);

        result.IsError.Should().BeFalse();
        slot.StartDateTime.Should().Be(newStart);
        slot.Capacity.Should().Be(10);
    }

    [Fact]
    public void Update_WhenStartAfterEnd_ReturnsInvalidTimeRange()
    {
        var slot = Slot.Create(1, Start, End, 5).Value;

        var result = slot.Update(End, Start, 5);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(SlotErrors.InvalidTimeRange);
    }

    [Fact]
    public void Update_WhenCapacityZero_ReturnsInvalidCapacity()
    {
        var slot = Slot.Create(1, Start, End, 5).Value;

        var result = slot.Update(Start, End, 0);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(SlotErrors.InvalidCapacity);
    }

    // ── Cancel ────────────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_WhenNotCancelled_SetsIsCancelled()
    {
        var slot = Slot.Create(1, Start, End, 5).Value;

        var result = slot.Cancel();

        result.IsError.Should().BeFalse();
        slot.IsCancelled.Should().BeTrue();
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ReturnsAlreadyCancelled()
    {
        var slot = Slot.Create(1, Start, End, 5).Value;
        slot.Cancel();

        var result = slot.Cancel();

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(SlotErrors.AlreadyCancelled);
    }

    // ── HasStarted ────────────────────────────────────────────────────────────

    [Fact]
    public void HasStarted_WhenBeforeStart_ReturnsFalse()
    {
        var slot = Slot.Create(1, Start, End, 5).Value;

        slot.HasStarted(Start.AddMinutes(-1)).Should().BeFalse();
    }

    [Fact]
    public void HasStarted_WhenAtStart_ReturnsTrue()
    {
        var slot = Slot.Create(1, Start, End, 5).Value;

        slot.HasStarted(Start).Should().BeTrue();
    }

    [Fact]
    public void HasStarted_WhenAfterStart_ReturnsTrue()
    {
        var slot = Slot.Create(1, Start, End, 5).Value;

        slot.HasStarted(Start.AddHours(1)).Should().BeTrue();
    }

    // ── OverlapsWith ──────────────────────────────────────────────────────────

    [Fact]
    public void OverlapsWith_WhenRangesOverlap_ReturnsTrue()
    {
        var slot = Slot.Create(1, Start, End, 5).Value;

        slot.OverlapsWith(Start.AddHours(1), End.AddHours(1)).Should().BeTrue();
    }

    [Fact]
    public void OverlapsWith_WhenRangeCompletelyContained_ReturnsTrue()
    {
        var slot = Slot.Create(1, Start, End, 5).Value;

        slot.OverlapsWith(Start.AddHours(1), Start.AddHours(3)).Should().BeTrue();
    }

    [Fact]
    public void OverlapsWith_WhenRangeAfterSlot_ReturnsFalse()
    {
        var slot = Slot.Create(1, Start, End, 5).Value;

        slot.OverlapsWith(End, End.AddHours(2)).Should().BeFalse();
    }

    [Fact]
    public void OverlapsWith_WhenRangeBeforeSlot_ReturnsFalse()
    {
        var slot = Slot.Create(1, Start, End, 5).Value;

        slot.OverlapsWith(Start.AddHours(-3), Start).Should().BeFalse();
    }

    // ── IsAlreadyBookedBy ─────────────────────────────────────────────────────

    [Fact]
    public void IsAlreadyBookedBy_WhenUserHasActiveBooking_ReturnsTrue()
    {
        var slot = Slot.Create(1, Start, End, 5).Value;
        var userId = Guid.NewGuid();
        var booking = CreateBookingForSlot(userId);
        slot.Bookings.Add(booking);

        slot.IsAlreadyBookedBy(userId).Should().BeTrue();
    }

    [Fact]
    public void IsAlreadyBookedBy_WhenUserHasCancelledBookingOnly_ReturnsFalse()
    {
        var slot = Slot.Create(1, Start, End, 5).Value;
        var userId = Guid.NewGuid();
        var booking = CreateBookingForSlot(userId, cancelled: true);
        slot.Bookings.Add(booking);

        slot.IsAlreadyBookedBy(userId).Should().BeFalse();
    }

    [Fact]
    public void IsAlreadyBookedBy_WhenUserHasNoBooking_ReturnsFalse()
    {
        var slot = Slot.Create(1, Start, End, 5).Value;

        slot.IsAlreadyBookedBy(Guid.NewGuid()).Should().BeFalse();
    }

    // ── IsFull ────────────────────────────────────────────────────────────────

    [Fact]
    public void IsFull_WhenCapacityNotReached_ReturnsFalse()
    {
        var slot = Slot.Create(1, Start, End, 2).Value;
        slot.Bookings.Add(CreateBookingForSlot(Guid.NewGuid()));

        slot.IsFull().Should().BeFalse();
    }

    [Fact]
    public void IsFull_WhenCapacityReached_ReturnsTrue()
    {
        var slot = Slot.Create(1, Start, End, 1).Value;
        slot.Bookings.Add(CreateBookingForSlot(Guid.NewGuid()));

        slot.IsFull().Should().BeTrue();
    }

    [Fact]
    public void IsFull_WhenCancelledBookingsOnly_ReturnsFalse()
    {
        var slot = Slot.Create(1, Start, End, 1).Value;
        slot.Bookings.Add(CreateBookingForSlot(Guid.NewGuid(), cancelled: true));

        slot.IsFull().Should().BeFalse();
    }

    // ── GetActiveBookerIds ────────────────────────────────────────────────────

    [Fact]
    public void GetActiveBookerIds_WhenMultipleActiveBookings_ReturnsDistinctIds()
    {
        var slot = Slot.Create(1, Start, End, 5).Value;
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        slot.Bookings.Add(CreateBookingForSlot(userId1));
        slot.Bookings.Add(CreateBookingForSlot(userId2));

        var result = slot.GetActiveBookerIds();

        result.Should().HaveCount(2);
        result.Should().Contain(userId1);
        result.Should().Contain(userId2);
    }

    [Fact]
    public void GetActiveBookerIds_WhenCancelledBooking_ExcludesIt()
    {
        var slot = Slot.Create(1, Start, End, 5).Value;
        var userId = Guid.NewGuid();
        slot.Bookings.Add(CreateBookingForSlot(userId, cancelled: true));

        var result = slot.GetActiveBookerIds();

        result.Should().BeEmpty();
    }

    private static Booking CreateBookingForSlot(Guid userId, bool cancelled = false)
    {
        var booking = (Booking)Activator.CreateInstance(typeof(Booking), nonPublic: true)!;
        typeof(Booking).GetProperty("UserId")!.SetValue(booking, userId);
        if (cancelled)
            typeof(Booking).GetProperty("CancelledAt")!.SetValue(booking, (DateTime?)DateTime.UtcNow.AddHours(-1));
        return booking;
    }
}
