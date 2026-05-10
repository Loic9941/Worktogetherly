using WorkTogetherly.Application.Workspaces.Common;

namespace WorkTogetherly.Presentation.Models.Workspace
{
    public record CreateWorkspaceRequest(
        string Name,
        string Description,
        string Address,
        double Latitude,
        double Longitude,
        int Capacity,
        bool IsActive,
        List<WorkspaceMaterialItem> Materials,
        List<int> RuleIds);

    public record UpdateWorkspaceRequest(
        string Name,
        string Description,
        string Address,
        double Latitude,
        double Longitude,
        int Capacity,
        bool IsActive,
        List<WorkspaceMaterialItem> Materials,
        List<int> RuleIds);
}
