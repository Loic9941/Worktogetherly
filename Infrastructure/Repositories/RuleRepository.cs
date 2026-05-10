using Microsoft.EntityFrameworkCore;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Interfaces;
using WorkTogetherly.Infrastructure.Persistence;

namespace WorkTogetherly.Infrastructure.Repositories;

public class RuleRepository : IRuleRepository
{
    private readonly AppDbContext _context;
    public RuleRepository(AppDbContext context) => _context = context;

    public Task<Rule?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Rules.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Rule>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Rules.OrderBy(r => r.Name).ToListAsync(cancellationToken);

    public Task AddAsync(Rule rule, CancellationToken cancellationToken = default)
    {
        _context.Rules.Add(rule);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
