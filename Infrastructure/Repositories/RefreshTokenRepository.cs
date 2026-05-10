using Microsoft.EntityFrameworkCore;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Infrastructure.Persistence;

namespace WorkTogetherly.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context) => _context = context;

    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default) =>
        _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token, cancellationToken);

    public Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _context.RefreshTokens.Add(refreshToken);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
