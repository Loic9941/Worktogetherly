using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Messages.Common;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Messages.GetUserMessages;

public class GetUserMessagesHandler(IMessageRepository messageRepository) : IRequestHandler<GetUserMessagesQuery, ErrorOr<List<MessageResult>>>
{
    private readonly IMessageRepository _messageRepository = messageRepository;

    public async Task<ErrorOr<List<MessageResult>>> Handle(GetUserMessagesQuery request, CancellationToken cancellationToken)
    {
        // Fetch messages for the user
        var messages = await _messageRepository.GetByRecipientIdAsync(request.UserId, cancellationToken);

        // Map messages to results and return
        return messages
            .Select(m => new MessageResult(m.Id, m.Content, m.IsRead, m.CreatedAt))
            .ToList();
    }
}
