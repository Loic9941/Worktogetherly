namespace WorkTogetherly.Domain.Events.Bookings;

public record BookingArrivalTimeUpdatedEvent(int BookingId, TimeOnly NewArrivalTime) : IDomainEvent;
