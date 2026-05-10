using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Users.GetCurrentUser;

namespace WorkTogetherly.Application.Users.DeleteUserPhoto
{
    public record DeleteUserPhotoCommand(Guid UserId) : IRequest<ErrorOr<CurrentUserResult>>;
}
