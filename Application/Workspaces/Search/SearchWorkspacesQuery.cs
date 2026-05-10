using ErrorOr;
using MediatR;
namespace WorkTogetherly.Application.Workspaces.Search
{
    public record SearchWorkspacesQuery(
        double Latitude,
        double Longitude,
        double RadiusKm,
        DateOnly Date,
        Guid UserId) : IRequest<ErrorOr<List<WorkspaceSearchResult>>>;
}
