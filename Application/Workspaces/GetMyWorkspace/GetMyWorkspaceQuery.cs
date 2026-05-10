using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Workspaces.Common;

namespace WorkTogetherly.Application.Workspaces.GetMyWorkspace
{
    public record GetMyWorkspaceQuery(Guid UserId) : IRequest<ErrorOr<WorkspaceResult?>>;
}
