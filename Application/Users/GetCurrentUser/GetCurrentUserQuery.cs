using ErrorOr;
using MediatR;

namespace WorkTogetherly.Application.Users.GetCurrentUser
{
    public record GetCurrentUserQuery(Guid UserId) : IRequest<ErrorOr<CurrentUserResult>>;
}
