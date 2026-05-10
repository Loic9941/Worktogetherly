namespace WorkTogetherly.Presentation.Models.Booking
{
    public record CreateBookingRequest(int SlotId, TimeOnly ArrivalTime, IReadOnlyList<int>? MaterialIds);
}
