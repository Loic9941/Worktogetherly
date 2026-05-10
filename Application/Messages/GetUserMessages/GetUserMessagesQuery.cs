using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Messages.Common;

namespace WorkTogetherly.Application.Messages.GetUserMessages;

public record GetUserMessagesQuery(Guid UserId) : IRequest<ErrorOr<List<MessageResult>>>;
