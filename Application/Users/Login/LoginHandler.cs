using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Errors;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Application.Users.Shared;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Users.Login
{
    public class LoginHandler(IUserRepository userRepository, ITokenService tokenService) : IRequestHandler<LoginCommand, ErrorOr<AuthResult>>
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly ITokenService _tokenService = tokenService;

        public async Task<ErrorOr<AuthResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user is null)
                return UserErrors.InvalidCredentials;

            bool valid = await _userRepository.ValidatePasswordAsync(user, request.Password);
            if (!valid)
                return UserErrors.InvalidCredentials;

            return await _tokenService.GenerateTokensAsync(user);
        }
    }
}
