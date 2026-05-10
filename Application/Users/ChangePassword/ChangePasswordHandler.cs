using ErrorOr;
using MediatR;
using WorkTogetherly.Domain.Errors;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Users.ChangePassword
{
    public class ChangePasswordHandler(IUserRepository userRepository) : IRequestHandler<ChangePasswordCommand, ErrorOr<Updated>>
    {
        private readonly IUserRepository _userRepository = userRepository;

        public async Task<ErrorOr<Updated>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            // Fetch the user
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            
            // Validate user existence
            if (user is null)
                return UserErrors.NotFound;

            // Attempt to change the password and return the result
            return await _userRepository.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        }
    }
}
