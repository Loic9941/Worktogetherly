using ErrorOr;
using MediatR;

namespace WorkTogetherly.Application.Users.UpdateUser
{
    public record UpdateUserCommand(
        Guid UserId,
        string FirstName,
        string LastName
    ) : IRequest<ErrorOr<Updated>>;
}
