using Microsoft.EntityFrameworkCore;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Interfaces;
using WorkTogetherly.Infrastructure.Persistence;

namespace WorkTogetherly.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _context;
    public ReviewRepository(AppDbContext context) => _context = context;

    public Task<Review?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Workspace)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<Review?> GetByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default) =>
        _context.Reviews.FirstOrDefaultAsync(r => r.BookingId == bookingId, cancellationToken);

    public Task<Review?> GetByReviewerIdAndWorkspaceIdAsync(Guid reviewerId, int workspaceId, CancellationToken cancellationToken = default) =>
        _context.Reviews.FirstOrDefaultAsync(r => r.ReviewerId == reviewerId && r.WorkspaceId == workspaceId, cancellationToken);

    public async Task<IReadOnlyList<Review>> GetByWorkspaceIdAsync(int workspaceId, CancellationToken cancellationToken = default) =>
        await _context.Reviews
            .Include(r => r.Reviewer)
            .Where(r => r.WorkspaceId == workspaceId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task AddAsync(Review review, CancellationToken cancellationToken = default)
    {
        _context.Reviews.Add(review);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
