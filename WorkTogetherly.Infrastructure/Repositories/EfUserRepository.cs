using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkTogetherly.Application.Repositories;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Infrastructure.Persistence;

namespace WorkTogetherly.Infrastructure.Repositories;

public class EfUserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public EfUserRepository(AppDbContext db) => _db = db;

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _db.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _db.Users.Add(user);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
