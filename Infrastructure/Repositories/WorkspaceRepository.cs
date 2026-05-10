using Microsoft.EntityFrameworkCore;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Interfaces;
using WorkTogetherly.Infrastructure.Persistence;

namespace WorkTogetherly.Infrastructure.Repositories;

public class WorkspaceRepository : IWorkspaceRepository
{
    private readonly AppDbContext _context;
    public WorkspaceRepository(AppDbContext context) => _context = context;

    public Task<Workspace?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Workspaces
            .Include(w => w.WorkspaceMaterials).ThenInclude(wm => wm.Material)
            .Include(w => w.WorkspaceRules).ThenInclude(wr => wr.Rule)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Workspace>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _context.Workspaces
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);

    public Task<Workspace?> GetByUserIdWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _context.Workspaces
            .Include(w => w.WorkspaceMaterials).ThenInclude(wm => wm.Material)
            .Include(w => w.WorkspaceRules).ThenInclude(wr => wr.Rule)
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Workspace>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        await _context.Workspaces
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);

    public Task AddAsync(Workspace workspace, CancellationToken cancellationToken = default)
    {
        _context.Workspaces.Add(workspace);
        return Task.CompletedTask;
    }

    public async Task<List<Workspace>> SearchAsync(
        Guid guid,
        double latitude, double longitude, double radiusKm,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        // Bounding box pre-filter in SQL (~1 degree lat = 111 km)
        double latDelta = radiusKm / 111.0;
        double lngDelta = radiusKm / (111.0 * Math.Cos(latitude * Math.PI / 180.0));

        var candidates = await _context.Workspaces
            .Include(w => w.WorkspaceMaterials).ThenInclude(wm => wm.Material)
            .Include(w => w.WorkspaceRules).ThenInclude(wr => wr.Rule)
            .Include(w => w.Slots).ThenInclude(s => s.Bookings)
            .Where(w => w.IsActive
                && w.Latitude >= latitude - latDelta && w.Latitude <= latitude + latDelta
                && w.Longitude >= longitude - lngDelta && w.Longitude <= longitude + lngDelta
                && w.UserId != guid
                && w.Slots.Any(s =>
                    s.StartDateTime.Date == DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc).Date
                    && s.Bookings.Count(b => b.CancelledAt == null) < s.Capacity))
            .ToListAsync(cancellationToken);

        // Haversine refinement in memory
        return candidates
            .Where(w => HaversineKm(latitude, longitude, w.Latitude, w.Longitude) <= radiusKm)
            .OrderBy(w => HaversineKm(latitude, longitude, w.Latitude, w.Longitude))
            .ToList();
    }

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180)
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
