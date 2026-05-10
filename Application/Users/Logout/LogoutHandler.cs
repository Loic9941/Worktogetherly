using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Interfaces;

namespace WorkTogetherly.Application.Users.Logout
{
    public class LogoutHandler(ITokenService tokenService) : IRequestHandler<LogoutCommand, ErrorOr<bool>>
    {
        private readonly ITokenService _tokenService = tokenService;

        public async Task<ErrorOr<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            return await _tokenService.RevokeTokenAsync(request.RefreshToken);
        }
    }
}
