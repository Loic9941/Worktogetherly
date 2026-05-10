using WorkTogetherly.Shared.Services;

namespace WorkTogetherly.Services
{
    internal class SecureTokenStorage : ITokenStorage
    {
        private string? _accessToken;
        private const string RefreshTokenKey = "refreshToken";

        public string? GetAccessToken() => _accessToken;
        public void SetAccessToken(string token) => _accessToken = token;

        public async Task<string?> GetRefreshTokenAsync()
        {
            try { return await SecureStorage.GetAsync(RefreshTokenKey); }
            catch { return null; }
        }

        public async Task SetRefreshTokenAsync(string token)
        {
            try { await SecureStorage.SetAsync(RefreshTokenKey, token); }
            catch { }
        }

        public Task ClearAsync()
        {
            _accessToken = null;
            SecureStorage.Remove(RefreshTokenKey);
            return Task.CompletedTask;
        }
    }
}
