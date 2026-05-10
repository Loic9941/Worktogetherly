using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Common.Services;
using WorkTogetherly.Application.Workspaces.Common;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Workspaces.Search
{
    public class SearchWorkspacesHandler(IWorkspaceRepository workspaceRepository) : IRequestHandler<SearchWorkspacesQuery, ErrorOr<List<WorkspaceSearchResult>>>
    {
        private readonly IWorkspaceRepository _workspaceRepository = workspaceRepository;

        public async Task<ErrorOr<List<WorkspaceSearchResult>>> Handle(SearchWorkspacesQuery request, CancellationToken cancellationToken)
        {
            var workspaces = await _workspaceRepository.SearchAsync(
                request.UserId,
                request.Latitude,
                request.Longitude,
                request.RadiusKm,
                request.Date,
                cancellationToken);

            return workspaces
                .Select(w =>
                {
                    var (lat, lng) = GeoCalculator.ApproximateCoords(w.Id, w.Latitude, w.Longitude);
                    var distanceKm = Math.Round(GeoCalculator.DistanceKm(request.Latitude, request.Longitude, w.Latitude, w.Longitude), 1);
                    return new WorkspaceSearchResult(
                        w.Id,
                        w.Name,
                        w.Description,
                        lat,
                        lng,
                        w.Capacity,
                        w.PhotoPath,
                        distanceKm,
                        w.WorkspaceMaterials.Select(wm => new WorkspaceMaterialResult(wm.MaterialId, wm.Material.Name, wm.Quantity)).ToList(),
                        w.WorkspaceRules.Select(wr => new WorkspaceRuleResult(wr.RuleId, wr.Rule.Name)).ToList());
                }).ToList();
        }
    }
}
