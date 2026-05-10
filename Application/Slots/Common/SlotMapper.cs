using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Application.Slots.Common
{
    internal static class SlotMapper
    {
        internal static SlotResult ToResult(Slot slot)
        {
            var activeBookings = slot.Bookings.Where(b => !b.IsCancelled).ToList();
            var availablePlaces = slot.Capacity - activeBookings.Count;

            var attendees = activeBookings
                .Where(b => b.User is not null)
                .Select(b => new AttendeeResult(
                    b.User!.FirstName,
                    b.User.LastName.Length > 0 ? b.User.LastName[0].ToString() : string.Empty))
                .ToList();

            return new SlotResult(
                slot.Id,
                slot.WorkspaceId,
                slot.StartDateTime,
                slot.EndDateTime,
                slot.Capacity,
                availablePlaces,
                attendees);
        }
    }
}
