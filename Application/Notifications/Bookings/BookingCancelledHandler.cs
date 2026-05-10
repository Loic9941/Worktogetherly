using MediatR;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Application.Messages.Common;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Events.Bookings;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Notifications.Bookings;

internal sealed class BookingCancelledHandler(
    ISlotRepository slotRepository,
    IMessageRepository messageRepository,
    INotificationService notificationService,
    IUserRepository userRepository,
    IEmailService emailService) : INotificationHandler<BookingCancelledEvent>
{
    public async Task Handle(BookingCancelledEvent notification, CancellationToken cancellationToken)
    {
        var slot = await slotRepository.GetByIdAsync(notification.SlotId, cancellationToken);
        if (slot?.Workspace is null)
            return;

        var ownerId = slot.Workspace.UserId;
        var content = $"Une réservation a été annulée pour votre espace « {slot.Workspace.Name} ».";

        var message = Message.Create(
            senderId: notification.UserId,
            recipientId: ownerId,
            content: content);

        await messageRepository.AddAsync(message, cancellationToken);
        await messageRepository.SaveChangesAsync(cancellationToken);

        await notificationService.SendToUserAsync(
            ownerId,
            new MessageResult(message.Id, message.Content, message.IsRead, message.CreatedAt),
            cancellationToken);

        var owner = await userRepository.GetByIdAsync(ownerId, cancellationToken);
        if (owner?.Email is not null)
            await emailService.SendAsync(
                owner.Email,
                $"Réservation annulée — {slot.Workspace.Name}",
                $"<p>{content}</p>",
                cancellationToken);
    }
}
