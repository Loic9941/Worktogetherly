using Microsoft.EntityFrameworkCore;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Interfaces;
using WorkTogetherly.Infrastructure.Persistence;

namespace WorkTogetherly.Infrastructure.Repositories;

public class MaterialRepository : IMaterialRepository
{
    private readonly AppDbContext _context;
    public MaterialRepository(AppDbContext context) => _context = context;

    public Task<Material?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Materials.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Material>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Materials.OrderBy(m => m.Name).ToListAsync(cancellationToken);

    public Task AddAsync(Material material, CancellationToken cancellationToken = default)
    {
        _context.Materials.Add(material);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
