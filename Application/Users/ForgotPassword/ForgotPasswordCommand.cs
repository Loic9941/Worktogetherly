using ErrorOr;
using MediatR;

namespace WorkTogetherly.Application.Users.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<ErrorOr<Success>>;
