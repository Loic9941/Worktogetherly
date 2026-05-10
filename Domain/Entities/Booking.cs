using ErrorOr;
using WorkTogetherly.Domain.Errors;
using WorkTogetherly.Domain.Events.Bookings;
using WorkTogetherly.Domain.Primitives;

namespace WorkTogetherly.Domain.Entities;

public class Booking : AggregateRoot
{
    public int Id { get; private set; }
    public int SlotId { get; private set; }
    public Guid UserId { get; private set; }
    public TimeOnly ArrivalTime { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    public bool IsCancelled => CancelledAt.HasValue;

    public Slot Slot { get; private set; } = null!;
    public User User { get; private set; } = null!;
    public Review? Review { get; private set; }
    public ICollection<BookingMaterial> BookingMaterials { get; private set; } = [];

    private Booking() { }

    public static ErrorOr<Booking> Create(int slotId, Guid userId, TimeOnly arrivalTime, DateTime slotStart, DateTime slotEnd)
    {
        // Rebuild a full DateTime from the slot's date + the TimeOnly so we can compare against slotStart/slotEnd.
        var arrivalDateTime = slotStart.Date + arrivalTime.ToTimeSpan();
        if (arrivalDateTime < slotStart || arrivalDateTime > slotEnd)
            return BookingErrors.ArrivalTimeOutOfRange;

        var booking = new Booking
        {
            SlotId = slotId,
            UserId = userId,
            ArrivalTime = arrivalTime,
            CreatedAt = DateTime.UtcNow
        };
        booking.RaiseDomainEvent(new BookingCreatedEvent(booking.Id, slotId, userId));
        return booking;
    }

    public ErrorOr<Success> Cancel()
    {
        if (IsCancelled)
            return BookingErrors.AlreadyCancelled;

        CancelledAt = DateTime.UtcNow;
        RaiseDomainEvent(new BookingCancelledEvent(Id, SlotId, UserId));
        return Result.Success;
    }

    public ErrorOr<Success> UpdateArrivalTime(TimeOnly newArrivalTime, DateTime slotStart, DateTime slotEnd)
    {
        if (IsCancelled)
            return BookingErrors.AlreadyCancelled;

        var arrivalDateTime = slotStart.Date + newArrivalTime.ToTimeSpan();
        if (arrivalDateTime < slotStart || arrivalDateTime > slotEnd)
            return BookingErrors.ArrivalTimeOutOfRange;

        ArrivalTime = newArrivalTime;
        RaiseDomainEvent(new BookingArrivalTimeUpdatedEvent(Id, newArrivalTime));
        return Result.Success;
    }

    public bool HasArrivalTimePassed(DateTime now)
    {
        var arrivalDateTime = Slot.StartDateTime.Date + ArrivalTime.ToTimeSpan();
        // <= so that hitting the exact arrival time also blocks edits (you're already there).
        return arrivalDateTime <= now;
    }

    public bool IsSlotEnded(DateTime now) => Slot.EndDateTime < now;

    public void AddMaterials(IEnumerable<int> materialIds)
    {
        foreach (var materialId in materialIds)
            BookingMaterials.Add(BookingMaterial.Create(Id, materialId));
    }
}
