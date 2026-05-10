using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Users.Shared;

namespace WorkTogetherly.Application.Users.Register
{
    public record RegisterCommand(string Email, string Password, string FirstName, string LastName)
        : IRequest<ErrorOr<AuthResult>>;
}
