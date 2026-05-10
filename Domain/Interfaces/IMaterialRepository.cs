using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Domain.Interfaces;

public interface IMaterialRepository
{
    Task<Material?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Material>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Material material, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
