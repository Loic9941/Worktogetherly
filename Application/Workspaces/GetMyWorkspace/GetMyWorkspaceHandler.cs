using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Workspaces.Common;
using WorkTogetherly.Application.Workspaces.CreateWorkspace;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Workspaces.GetMyWorkspace
{
    public class GetMyWorkspaceHandler(IWorkspaceRepository workspaceRepository) : IRequestHandler<GetMyWorkspaceQuery, ErrorOr<WorkspaceResult?>>
    {
        private readonly IWorkspaceRepository _workspaceRepository = workspaceRepository;

        public async Task<ErrorOr<WorkspaceResult?>> Handle(GetMyWorkspaceQuery request, CancellationToken cancellationToken)
        {
            // Fetch the workspace by user ID with details
            var workspace = await _workspaceRepository.GetByUserIdWithDetailsAsync(request.UserId, cancellationToken);

            // If no workspace is found, return null
            if (workspace is null)
                return (WorkspaceResult?)null;

            return CreateWorkspaceHandler.MapResult(workspace);
        }
    }
}
