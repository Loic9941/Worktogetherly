using MediatR;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Application.Messages.Common;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Events.Slots;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Notifications.Slots;

internal sealed class SlotUpdatedHandler(
    ISlotRepository slotRepository,
    IMessageRepository messageRepository,
    INotificationService notificationService,
    IUserRepository userRepository,
    IEmailService emailService) : INotificationHandler<SlotUpdatedEvent>
{
    public async Task Handle(SlotUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var slot = await slotRepository.GetByIdAsync(notification.SlotId, cancellationToken);
        if (slot?.Workspace is null)
            return;

        var bookers = slot.GetActiveBookerIds();

        foreach (var bookerId in bookers)
        {
            var content = $"Le créneau du {slot.StartDateTime:dd/MM/yyyy} ({slot.StartDateTime:HH\\:mm}–{slot.EndDateTime:HH\\:mm}) pour l'espace « {slot.Workspace.Name} » a été modifié.";

            var message = Message.Create(
                senderId: slot.Workspace.UserId,
                recipientId: bookerId,
                content: content);

            await messageRepository.AddAsync(message, cancellationToken);
            await messageRepository.SaveChangesAsync(cancellationToken);

            await notificationService.SendToUserAsync(
                bookerId,
                new MessageResult(message.Id, message.Content, message.IsRead, message.CreatedAt),
                cancellationToken);

            var booker = await userRepository.GetByIdAsync(bookerId, cancellationToken);
            if (booker?.Email is not null)
                await emailService.SendAsync(
                    booker.Email,
                    $"Créneau modifié — {slot.Workspace.Name}",
                    $"<p>{content}</p>",
                    cancellationToken);
        }
    }
}
