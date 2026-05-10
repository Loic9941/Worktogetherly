using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Users.Shared;

namespace WorkTogetherly.Application.Users.RefreshToken
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<ErrorOr<AuthResult>>;
}
