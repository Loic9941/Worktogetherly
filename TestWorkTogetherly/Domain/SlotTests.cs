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
}
