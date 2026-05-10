using ErrorOr;
using MediatR;

namespace WorkTogetherly.Application.Users.Logout
{
    public record LogoutCommand(string RefreshToken) : IRequest<ErrorOr<bool>>;
}
