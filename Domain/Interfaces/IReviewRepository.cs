using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Domain.Interfaces
{
    public interface IReviewRepository
    {
        Task<Review?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Review?> GetByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Review>> GetByWorkspaceIdAsync(int workspaceId, CancellationToken cancellationToken = default);
        Task<Review?> GetByReviewerIdAndWorkspaceIdAsync(Guid reviewerId, int workspaceId, CancellationToken cancellationToken = default);
        Task AddAsync(Review review, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
