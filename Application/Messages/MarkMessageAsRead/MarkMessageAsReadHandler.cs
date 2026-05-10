using ErrorOr;
using MediatR;
using WorkTogetherly.Domain.Errors;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Messages.MarkMessageAsRead;

public class MarkMessageAsReadHandler(IMessageRepository messageRepository) : IRequestHandler<MarkMessageAsReadCommand, ErrorOr<Success>>
{
    private readonly IMessageRepository _messageRepository = messageRepository;

    public async Task<ErrorOr<Success>> Handle(MarkMessageAsReadCommand request, CancellationToken cancellationToken)
    {
        // Fetch the message
        var message = await _messageRepository.GetByIdAsync(request.MessageId, cancellationToken);
        
        // Validate message existence and ownership
        if (message is null || message.RecipientId != request.UserId)
            return MessageErrors.NotFound;

        // Mark the message as read and persist changes
        message.MarkAsRead();
        await _messageRepository.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
