using WorkTogetherly.Application.Messages.Common;

namespace WorkTogetherly.Application.Interfaces;

public interface INotificationService
{
    Task SendToUserAsync(Guid recipientId, MessageResult message, CancellationToken cancellationToken = default);
}
