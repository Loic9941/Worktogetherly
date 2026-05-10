namespace WorkTogetherly.Application.Bookings.Common
{
    public record BookingResult(
        int Id,
        int SlotId,
        DateTime SlotStart,
        DateTime SlotEnd,
        TimeOnly ArrivalTime,
        bool IsCancelled,
        WorkspaceSummary Workspace,
        bool HasReview);

    public record WorkspaceSummary(int Id, string Name, string Address);
}
