namespace WorkTogetherly.Application.Workspaces.GetWorkspaceDetails
{
    public record WorkspaceDetailsResult(
        int Id,
        string Name,
        string Description,
        double ApproxLatitude,
        double ApproxLongitude,
        double AverageRating,
        int ReviewCount,
        bool IsOwner,
        IReadOnlyList<WorkspaceMaterialDetailResult> Materials,
        IReadOnlyList<WorkspaceRuleDetailResult> Rules,
        IReadOnlyList<SlotDetailResult> Slots);

    public record WorkspaceMaterialDetailResult(int Id, string Name, int Quantity);

    public record WorkspaceRuleDetailResult(int Id, string Name);

    public record MaterialAvailabilityResult(int MaterialId, string Name, int TotalQuantity, int Available);

    public record SlotDetailResult(
        int Id,
        DateTime StartDateTime,
        DateTime EndDateTime,
        int Capacity,
        int AvailablePlaces,
        bool UserHasBooked,
        IReadOnlyList<MaterialAvailabilityResult> MaterialAvailabilities);
}
