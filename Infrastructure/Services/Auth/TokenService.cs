using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Application.Users.Shared;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Infrastructure.Services.Auth
{
    public class TokenService : ITokenService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public TokenService(
            IRefreshTokenRepository refreshTokenRepository,
            IUserRepository userRepository,
            IConfiguration configuration)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<AuthResult> GenerateTokensAsync(User user)
        {
            var accessToken = GenerateJwt(user);
            var refreshToken = RefreshToken.Create(user.Id);

            await _refreshTokenRepository.AddAsync(refreshToken);
            await _refreshTokenRepository.SaveChangesAsync();

            return new AuthResult(
                accessToken,
                refreshToken.Token,
                DateTime.UtcNow.AddMinutes(60));
        }

        public async Task<ErrorOr<AuthResult>> RefreshTokenAsync(string refreshToken)
        {
            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

            if (token is null || !token.IsValid)
                return Application.Errors.UserErrors.InvalidRefreshToken;

            var user = await _userRepository.GetByIdAsync(token.UserId);
            if (user is null)
                return Domain.Errors.UserErrors.NotFound;

            token.Revoke();

            return await GenerateTokensAsync(user);
        }

        public async Task<ErrorOr<bool>> RevokeTokenAsync(string refreshToken)
        {
            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

            if (token is null || !token.IsValid)
                return Application.Errors.UserErrors.InvalidRefreshToken;

            token.Revoke();
            await _refreshTokenRepository.SaveChangesAsync();

            return true;
        }

        private string GenerateJwt(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
