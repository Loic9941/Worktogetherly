using Microsoft.AspNetCore.SignalR;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Application.Messages.Common;
using WorkTogetherly.Infrastructure.Hubs;

namespace WorkTogetherly.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToUserAsync(Guid recipientId, MessageResult message, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group($"user-{recipientId}")
            .SendAsync("ReceiveMessage", message, cancellationToken);
    }
}
