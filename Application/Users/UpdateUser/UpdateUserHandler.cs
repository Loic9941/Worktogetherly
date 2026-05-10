using ErrorOr;
using MediatR;
using WorkTogetherly.Domain.Errors;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Users.UpdateUser
{
    public class UpdateUserHandler(IUserRepository userRepository) : IRequestHandler<UpdateUserCommand, ErrorOr<Updated>>
    {
        private readonly IUserRepository _userRepository = userRepository;

        public async Task<ErrorOr<Updated>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
                return UserErrors.NotFound;

            user.UpdateProfile(request.FirstName, request.LastName);
            await _userRepository.UpdateAsync(user, cancellationToken);

            return Result.Updated;
        }
    }
}
