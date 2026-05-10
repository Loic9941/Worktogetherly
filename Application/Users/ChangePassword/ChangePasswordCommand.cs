using ErrorOr;
using MediatR;

namespace WorkTogetherly.Application.Users.ChangePassword
{
    public record ChangePasswordCommand(
        Guid UserId,
        string CurrentPassword,
        string NewPassword
    ) : IRequest<ErrorOr<Updated>>;
}
