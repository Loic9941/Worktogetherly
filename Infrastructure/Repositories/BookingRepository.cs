using Microsoft.EntityFrameworkCore;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Interfaces;
using WorkTogetherly.Infrastructure.Persistence;

namespace WorkTogetherly.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;
    public BookingRepository(AppDbContext context) => _context = context;

    public Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Bookings
            .Include(b => b.Slot).ThenInclude(s => s.Workspace)
            .Include(b => b.User)
            .Include(b => b.Review)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Booking>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _context.Bookings
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Booking>> GetByUserIdWithDetailsAsync(Guid userId, DateTime from, DateTime to, CancellationToken cancellationToken = default) =>
        await _context.Bookings
            .Include(b => b.Slot).ThenInclude(s => s.Workspace)
            .Include(b => b.Review)
            .Where(b => b.UserId == userId
                     && b.Slot.StartDateTime >= from
                     && b.Slot.StartDateTime <= to)
            .OrderBy(b => b.Slot.StartDateTime)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Booking>> GetBySlotIdAsync(int slotId, CancellationToken cancellationToken = default) =>
        await _context.Bookings
            .Where(b => b.SlotId == slotId)
            .ToListAsync(cancellationToken);

    public Task<int> CountActiveBySlotIdAsync(int slotId, CancellationToken cancellationToken = default) =>
        _context.Bookings
            .CountAsync(b => b.SlotId == slotId && b.CancelledAt == null, cancellationToken);

    public Task<int> CountActiveMaterialBookingsBySlotIdAsync(int slotId, int materialId, CancellationToken cancellationToken = default) =>
        _context.Bookings
            .CountAsync(b => b.SlotId == slotId
                          && b.CancelledAt == null
                          && b.BookingMaterials.Any(bm => bm.MaterialId == materialId), cancellationToken);

    public Task AddAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        _context.Bookings.Add(booking);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
