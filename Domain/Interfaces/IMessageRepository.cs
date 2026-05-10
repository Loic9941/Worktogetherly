using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Domain.Interfaces
{
    public interface IMessageRepository
    {
        Task<Message?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Message>> GetByRecipientIdAsync(Guid recipientId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Message>> GetBySenderIdAsync(Guid senderId, CancellationToken cancellationToken = default);
        Task<int> CountUnreadByRecipientIdAsync(Guid recipientId, CancellationToken cancellationToken = default);
        Task AddAsync(Message message, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
