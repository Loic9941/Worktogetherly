using ErrorOr;
using MediatR;
using WorkTogetherly.Domain.Errors;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Users.ResetPassword;

public class ResetPasswordHandler(IUserRepository userRepository) : IRequestHandler<ResetPasswordCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
            return UserErrors.NotFound;

        return await userRepository.ResetPasswordAsync(user, request.Token, request.NewPassword);
    }
}
