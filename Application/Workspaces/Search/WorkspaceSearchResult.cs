using WorkTogetherly.Application.Workspaces.Common;

namespace WorkTogetherly.Application.Workspaces.Search
{
    public record WorkspaceSearchResult(
        int Id,
        string Name,
        string Description,
        double Latitude,
        double Longitude,
        int Capacity,
        string? PhotoPath,
        double? DistanceKm,
        IReadOnlyList<WorkspaceMaterialResult> Materials,
        IReadOnlyList<WorkspaceRuleResult> Rules);
}
