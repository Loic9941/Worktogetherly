namespace WorkTogetherly.Application.Workspaces.Common
{
    public record WorkspaceMaterialItem(int MaterialId, int Quantity);

    public record WorkspaceResult(
        int Id,
        Guid? UserId,
        string Name,
        string Description,
        string Address,
        double Latitude,
        double Longitude,
        int Capacity,
        bool IsActive,
        DateTime CreatedAt,
        IReadOnlyList<WorkspaceMaterialResult> Materials,
        IReadOnlyList<WorkspaceRuleResult> Rules,
        string? PhotoPath);

    public record WorkspaceMaterialResult(int MaterialId, string Name, int Quantity);

    public record WorkspaceRuleResult(int RuleId, string Name);
}
