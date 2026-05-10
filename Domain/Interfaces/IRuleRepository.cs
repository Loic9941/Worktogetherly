using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Domain.Interfaces;

public interface IRuleRepository
{
    Task<Rule?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Rule>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Rule rule, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
