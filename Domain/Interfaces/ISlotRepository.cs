using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Domain.Interfaces
{
    public interface ISlotRepository
    {
        Task<Slot?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Slot>> GetByWorkspaceIdAsync(int workspaceId, CancellationToken cancellationToken = default);
        Task AddAsync(Slot slot, CancellationToken cancellationToken = default);
        Task UpdateAsync(Slot slot, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
