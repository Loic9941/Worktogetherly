using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Workspaces.Common;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Workspaces.CreateWorkspace
{
    public class CreateWorkspaceHandler(IWorkspaceRepository workspaceRepository) : IRequestHandler<CreateWorkspaceCommand, ErrorOr<WorkspaceResult>>
    {
        private readonly IWorkspaceRepository _workspaceRepository = workspaceRepository;

        public async Task<ErrorOr<WorkspaceResult>> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
        {
            var workspace = Workspace.Create(
                request.UserId,
                request.Name,
                request.Description,
                request.Address,
                request.Latitude,
                request.Longitude,
                request.Capacity,
                request.IsActive);

            workspace.ReplaceMaterials(request.Materials.Select(m => (m.MaterialId, m.Quantity)));
            workspace.ReplaceRules(request.RuleIds);

            await _workspaceRepository.AddAsync(workspace, cancellationToken);
            await _workspaceRepository.SaveChangesAsync(cancellationToken);

            var saved = await _workspaceRepository.GetByIdAsync(workspace.Id, cancellationToken);
            return MapResult(saved!);
        }

        internal static WorkspaceResult MapResult(Domain.Entities.Workspace w) => new(
            w.Id,
            w.UserId,
            w.Name,
            w.Description,
            w.Address,
            w.Latitude,
            w.Longitude,
            w.Capacity,
            w.IsActive,
            w.CreatedAt,
            w.WorkspaceMaterials.Select(wm => new WorkspaceMaterialResult(wm.MaterialId, wm.Material.Name, wm.Quantity)).ToList(),
            w.WorkspaceRules.Select(wr => new WorkspaceRuleResult(wr.RuleId, wr.Rule.Name)).ToList(),
            w.PhotoPath);
    }
}
