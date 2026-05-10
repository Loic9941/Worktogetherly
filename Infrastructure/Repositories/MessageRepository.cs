using Microsoft.EntityFrameworkCore;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Interfaces;
using WorkTogetherly.Infrastructure.Persistence;

namespace WorkTogetherly.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;
    public MessageRepository(AppDbContext context) => _context = context;

    public Task<Message?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Recipient)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Message>> GetByRecipientIdAsync(Guid recipientId, CancellationToken cancellationToken = default) =>
        await _context.Messages
            .Include(m => m.Sender)
            .Where(m => m.RecipientId == recipientId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Message>> GetBySenderIdAsync(Guid senderId, CancellationToken cancellationToken = default) =>
        await _context.Messages
            .Include(m => m.Recipient)
            .Where(m => m.SenderId == senderId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<int> CountUnreadByRecipientIdAsync(Guid recipientId, CancellationToken cancellationToken = default) =>
        _context.Messages.CountAsync(m => m.RecipientId == recipientId && !m.IsRead, cancellationToken);

    public Task AddAsync(Message message, CancellationToken cancellationToken = default)
    {
        _context.Messages.Add(message);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
