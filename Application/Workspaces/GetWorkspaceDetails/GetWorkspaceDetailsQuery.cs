using ErrorOr;
using MediatR;

namespace WorkTogetherly.Application.Workspaces.GetWorkspaceDetails
{
    public record GetWorkspaceDetailsQuery(
        int WorkspaceId,
        Guid UserId,
        DateOnly Date) : IRequest<ErrorOr<WorkspaceDetailsResult>>;
}
