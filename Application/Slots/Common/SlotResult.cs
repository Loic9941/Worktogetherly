namespace WorkTogetherly.Application.Slots.Common
{
    public record SlotResult(
        int Id,
        int WorkspaceId,
        DateTime StartDateTime,
        DateTime EndDateTime,
        int Capacity,
        int AvailablePlaces,
        IReadOnlyList<AttendeeResult> Attendees);

    public record AttendeeResult(string FirstName, string LastInitial, TimeOnly ArrivalTime);
}
