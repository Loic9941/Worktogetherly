using MediatR;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Application.Messages.Common;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Events.Bookings;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Notifications.Bookings;

internal sealed class BookingArrivalTimeUpdatedHandler(
    IBookingRepository bookingRepository,
    IMessageRepository messageRepository,
    INotificationService notificationService,
    IUserRepository userRepository,
    IEmailService emailService) : INotificationHandler<BookingArrivalTimeUpdatedEvent>
{
    public async Task Handle(BookingArrivalTimeUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(notification.BookingId, cancellationToken);
        if (booking?.Slot?.Workspace is null)
            return;

        var ownerId = booking.Slot.Workspace.UserId;
        var content = $"L'heure d'arrivée d'une réservation a été modifiée pour votre espace « {booking.Slot.Workspace.Name} » (nouvelle heure : {notification.NewArrivalTime:HH\\:mm}).";

        var message = Message.Create(
            senderId: booking.UserId,
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
                $"Heure d'arrivée modifiée — {booking.Slot.Workspace.Name}",
                $"<p>{content}</p>",
                cancellationToken);
    }
}
