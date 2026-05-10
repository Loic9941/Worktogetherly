using MediatR;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Application.Messages.Common;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Events.Bookings;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Notifications.Bookings;

internal sealed class BookingCreatedHandler(
    ISlotRepository slotRepository,
    IMessageRepository messageRepository,
    INotificationService notificationService,
    IUserRepository userRepository,
    IEmailService emailService) : INotificationHandler<BookingCreatedEvent>
{
    // Triggered by AppDbContext after SaveChanges — the booking is already in the DB at this point.
    public async Task Handle(BookingCreatedEvent notification, CancellationToken cancellationToken)
    {
        var slot = await slotRepository.GetByIdAsync(notification.SlotId, cancellationToken);
        // Slot or workspace could be gone if deleted between the booking and this handler — skip silently.
        if (slot?.Workspace is null)
            return;

        var ownerId = slot.Workspace.UserId;
        var content = $"Nouvelle réservation effectuée pour votre espace « {slot.Workspace.Name} ».";

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
                $"Nouvelle réservation — {slot.Workspace.Name}",
                $"<p>{content}</p>",
                cancellationToken);
    }
}
