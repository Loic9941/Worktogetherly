using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Workspaces.Common;
using WorkTogetherly.Application.Workspaces.CreateWorkspace;
using WorkTogetherly.Domain.Errors;
using WorkTogetherly.Domain.Interfaces;
using AppErrors = WorkTogetherly.Application.Errors;

namespace WorkTogetherly.Application.Workspaces.UpdateWorkspace
{
    public class UpdateWorkspaceHandler(IWorkspaceRepository workspaceRepository) : IRequestHandler<UpdateWorkspaceCommand, ErrorOr<WorkspaceResult>>
    {
        private readonly IWorkspaceRepository _workspaceRepository = workspaceRepository;

        public async Task<ErrorOr<WorkspaceResult>> Handle(UpdateWorkspaceCommand request, CancellationToken cancellationToken)
        {
            var workspace = await _workspaceRepository.GetByIdAsync(request.WorkspaceId, cancellationToken);
            if (workspace is null)
                return WorkspaceErrors.NotFound;

            if (workspace.UserId != request.UserId)
                return AppErrors.WorkspaceErrors.Unauthorized;

            workspace.Update(
                request.Name,
                request.Description,
                request.Address,
                request.Latitude,
                request.Longitude,
                request.Capacity,
                request.IsActive);

            workspace.ReplaceMaterials(request.Materials.Select(m => (m.MaterialId, m.Quantity)));
            workspace.ReplaceRules(request.RuleIds);

            await _workspaceRepository.SaveChangesAsync(cancellationToken);

            var saved = await _workspaceRepository.GetByIdAsync(workspace.Id, cancellationToken);
            return CreateWorkspaceHandler.MapResult(saved!);
        }
    }
}
