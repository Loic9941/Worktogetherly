using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Domain.Interfaces
{
    public interface IWorkspaceRepository
    {
        Task<Workspace?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Workspace>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Workspace?> GetByUserIdWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Workspace>> GetActiveAsync(CancellationToken cancellationToken = default);
        Task<List<Workspace>> SearchAsync(Guid userId, double latitude, double longitude, double radiusKm, DateOnly date, CancellationToken cancellationToken = default);
        Task AddAsync(Workspace workspace, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
