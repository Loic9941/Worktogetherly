using System.Threading;
using System.Threading.Tasks;
using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Application.Repositories;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
