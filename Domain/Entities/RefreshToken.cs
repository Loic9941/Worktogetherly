using System.Security.Cryptography;

namespace WorkTogetherly.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Token { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public bool IsRevoked { get; private set; }

        private RefreshToken() { }

        public static RefreshToken Create(Guid userId, int expiryDays = 30)
        {
            return new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
                IsRevoked = false
            };
        }

        public bool IsValid => !IsRevoked && ExpiresAt > DateTime.UtcNow;

        public void Revoke() => IsRevoked = true;
    }
}
