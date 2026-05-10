using ErrorOr;
using MediatR;

namespace WorkTogetherly.Application.Users.ResetPassword;

public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<ErrorOr<Success>>;
