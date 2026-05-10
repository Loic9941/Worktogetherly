namespace WorkTogetherly.Domain.Events.Bookings;

public record BookingCancelledEvent(int BookingId, int SlotId, Guid UserId) : IDomainEvent;
