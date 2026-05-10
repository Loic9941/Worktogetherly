using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Application.Users.Shared;

namespace WorkTogetherly.Application.Users.RefreshToken
{
    public class RefreshTokenHandler(ITokenService tokenService) : IRequestHandler<RefreshTokenCommand, ErrorOr<AuthResult>>
    {
        private readonly ITokenService _tokenService = tokenService;

        public async Task<ErrorOr<AuthResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            return await _tokenService.RefreshTokenAsync(request.RefreshToken);
        }
    }
}
