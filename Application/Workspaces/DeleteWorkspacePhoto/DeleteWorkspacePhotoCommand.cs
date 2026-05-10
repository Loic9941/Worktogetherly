using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Workspaces.Common;

namespace WorkTogetherly.Application.Workspaces.DeleteWorkspacePhoto
{
    public record DeleteWorkspacePhotoCommand(
        int WorkspaceId,
        Guid UserId) : IRequest<ErrorOr<WorkspaceResult>>;
}
