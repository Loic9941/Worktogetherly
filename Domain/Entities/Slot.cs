using ErrorOr;
using WorkTogetherly.Domain.Errors;
using WorkTogetherly.Domain.Events.Slots;
using WorkTogetherly.Domain.Primitives;

namespace WorkTogetherly.Domain.Entities;

public class Slot : AggregateRoot
{
    public int Id { get; private set; }
    public int WorkspaceId { get; private set; }
    public DateTime StartDateTime { get; private set; }
    public DateTime EndDateTime { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int Capacity { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    public bool IsCancelled => CancelledAt.HasValue;

    public Workspace Workspace { get; private set; } = null!;
    public ICollection<Booking> Bookings { get; private set; } = [];

    private Slot() { }

    public static ErrorOr<Slot> Create(int workspaceId, DateTime startDateTime, DateTime endDateTime, int capacity)
    {
        if (startDateTime >= endDateTime)
            return SlotErrors.InvalidTimeRange;
        if (capacity <= 0)
            return SlotErrors.InvalidCapacity;

        return new Slot
        {
            WorkspaceId = workspaceId,
            StartDateTime = startDateTime,
            EndDateTime = endDateTime,
            Capacity = capacity,
            CreatedAt = DateTime.UtcNow
        };
    }

    public ErrorOr<Success> Update(DateTime startDateTime, DateTime endDateTime, int capacity, DateTime now)
    {
        if (IsCancelled)
            return SlotErrors.AlreadyCancelled;
        if (HasStarted(now))
            return SlotErrors.AlreadyStarted;
        if (startDateTime >= endDateTime)
            return SlotErrors.InvalidTimeRange;
        if (capacity <= 0)
            return SlotErrors.InvalidCapacity;

        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
        Capacity = capacity;
        RaiseDomainEvent(new SlotUpdatedEvent(Id, WorkspaceId));
        return Result.Success;
    }

    public ErrorOr<Success> Cancel(DateTime now)
    {
        if (IsCancelled)
            return SlotErrors.AlreadyCancelled;
        if (HasStarted(now))
            return SlotErrors.AlreadyStarted;

        CancelledAt = now;
        RaiseDomainEvent(new SlotCancelledEvent(Id, WorkspaceId));
        return Result.Success;
    }

    public bool HasStarted(DateTime now) => StartDateTime <= now;

    public int AvailablePlaces()
    {
        var active = Bookings.Count(b => !b.IsCancelled);
        return Capacity - active;
    }

    public int AvailableMaterialQuantity(WorkspaceMaterial workspaceMaterial)
    {
        var booked = Bookings
            .Where(b => !b.IsCancelled)
            .SelectMany(b => b.BookingMaterials)
            .Count(bm => bm.MaterialId == workspaceMaterial.MaterialId);
        return Math.Max(0, workspaceMaterial.Quantity - booked);
    }

    public bool OverlapsWith(DateTime start, DateTime end) => start < EndDateTime && StartDateTime < end;

    public bool IsAlreadyBookedBy(Guid userId) =>
        Bookings.Any(b => b.UserId == userId && !b.IsCancelled);

    public bool IsFull() => Bookings.Count(b => !b.IsCancelled) >= Capacity;

    public IReadOnlyList<Guid> GetActiveBookerIds() =>
        Bookings
            .Where(b => b.CancelledAt is null)
            .Select(b => b.UserId)
            .Distinct()
            .ToList();
}
