using ErrorOr;
using MediatR;

namespace WorkTogetherly.Application.Messages.MarkMessageAsRead;

public record MarkMessageAsReadCommand(int MessageId, Guid UserId) : IRequest<ErrorOr<Success>>;
