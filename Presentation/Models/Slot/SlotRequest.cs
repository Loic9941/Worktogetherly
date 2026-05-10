namespace WorkTogetherly.Presentation.Models.Slot
{
    public record CreateSlotRequest(DateTime StartDateTime, DateTime EndDateTime, int Capacity);
    public record UpdateSlotRequest(DateTime StartDateTime, DateTime EndDateTime, int Capacity);
}
