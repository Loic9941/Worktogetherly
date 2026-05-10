using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Errors;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Application.Users.Shared;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Users.Register
{
    public class RegisterHandler(IUserRepository userRepository, ITokenService tokenService) : IRequestHandler<RegisterCommand, ErrorOr<AuthResult>>
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly ITokenService _tokenService = tokenService;

        public async Task<ErrorOr<AuthResult>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existing is not null)
                return UserErrors.EmailAlreadyExists;

            var user = User.Create(request.FirstName, request.LastName, request.Email);
            var createResult = await _userRepository.CreateWithPasswordAsync(user, request.Password, cancellationToken);
            if (createResult.IsError)
                return createResult.Errors;

            return await _tokenService.GenerateTokensAsync(createResult.Value);
        }
    }
}
