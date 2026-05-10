using Microsoft.EntityFrameworkCore;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Interfaces;
using WorkTogetherly.Infrastructure.Persistence;

namespace WorkTogetherly.Infrastructure.Repositories;

public class SlotRepository : ISlotRepository
{
    private readonly AppDbContext _context;
    public SlotRepository(AppDbContext context) => _context = context;

    public Task<Slot?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Slots
            .Include(s => s.Workspace)
            .Include(s => s.Bookings)
                .ThenInclude(b => b.User)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Slot>> GetByWorkspaceIdAsync(int workspaceId, CancellationToken cancellationToken = default) =>
        await _context.Slots
            .Where(s => s.WorkspaceId == workspaceId)
            .Include(s => s.Bookings)
                .ThenInclude(b => b.User)
            .Include(s => s.Bookings)
                .ThenInclude(b => b.BookingMaterials)
            .OrderBy(s => s.StartDateTime)
            .ToListAsync(cancellationToken);

    public Task AddAsync(Slot slot, CancellationToken cancellationToken = default)
    {
        _context.Slots.Add(slot);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Slot slot, CancellationToken cancellationToken = default)
    {
        _context.Slots.Update(slot);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
