using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Users.Common;
using WorkTogetherly.Domain.Errors;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Users.GetCurrentUser
{
    public class GetCurrentUserHandler(IUserRepository userRepository) : IRequestHandler<GetCurrentUserQuery, ErrorOr<CurrentUserResult>>
    {
        private readonly IUserRepository _userRepository = userRepository;

        public async Task<ErrorOr<CurrentUserResult>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
                return UserErrors.NotFound;

            return user.ToResult();
        }
    }
}
