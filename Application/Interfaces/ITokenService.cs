using ErrorOr;
using WorkTogetherly.Application.Users.Shared;
using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Application.Interfaces
{
    public interface ITokenService
    {
        Task<AuthResult> GenerateTokensAsync(User user);
        Task<ErrorOr<AuthResult>> RefreshTokenAsync(string refreshToken);
        Task<ErrorOr<bool>> RevokeTokenAsync(string refreshToken);
    }
}
