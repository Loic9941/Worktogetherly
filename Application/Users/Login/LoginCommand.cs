using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Users.Shared;

namespace WorkTogetherly.Application.Users.Login
{
    public record LoginCommand(string Email, string Password) : IRequest<ErrorOr<AuthResult>>;
}
