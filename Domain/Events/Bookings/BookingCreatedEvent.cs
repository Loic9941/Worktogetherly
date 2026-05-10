namespace WorkTogetherly.Domain.Events.Bookings;

public record BookingCreatedEvent(int BookingId, int SlotId, Guid UserId) : IDomainEvent;
