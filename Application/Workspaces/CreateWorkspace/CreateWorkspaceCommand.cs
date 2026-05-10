using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Workspaces.Common;

namespace WorkTogetherly.Application.Workspaces.CreateWorkspace
{
    public record CreateWorkspaceCommand(
        Guid UserId,
        string Name,
        string Description,
        string Address,
        double Latitude,
        double Longitude,
        int Capacity,
        bool IsActive,
        IReadOnlyList<WorkspaceMaterialItem> Materials,
        IReadOnlyList<int> RuleIds) : IRequest<ErrorOr<WorkspaceResult>>;
}
