using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Application.Bookings.Common;

public static class BookingMapper
{
    public static BookingResult ToResult(this Booking booking) => new(
        booking.Id,
        booking.SlotId,
        booking.Slot.StartDateTime,
        booking.Slot.EndDateTime,
        booking.ArrivalTime,
        booking.IsCancelled,
        new WorkspaceSummary(booking.Slot.Workspace.Id, booking.Slot.Workspace.Name, booking.Slot.Workspace.Address),
        booking.Review is not null);
}
