namespace WorkTogetherly.Shared.Models
{
    public record BookingDto(
        int Id,
        int SlotId,
        DateTime SlotStart,
        DateTime SlotEnd,
        TimeOnly ArrivalTime,
        bool IsCancelled,
        WorkspaceSummaryDto Workspace,
        bool HasReview);

    public record WorkspaceSummaryDto(int Id, string Name, string Address);
}
