using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Domain.Interfaces
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Booking>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Booking>> GetByUserIdWithDetailsAsync(Guid userId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Booking>> GetBySlotIdAsync(int slotId, CancellationToken cancellationToken = default);
        Task<int> CountActiveBySlotIdAsync(int slotId, CancellationToken cancellationToken = default);
        Task<int> CountActiveMaterialBookingsBySlotIdAsync(int slotId, int materialId, CancellationToken cancellationToken = default);
        Task AddAsync(Booking booking, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
