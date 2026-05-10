namespace WorkTogetherly.Shared.Services
{
    public class TokenStorageService
    {
        private readonly ITokenStorage _storage;

        public TokenStorageService(ITokenStorage storage)
        {
            _storage = storage;
        }

        public string? GetAccessToken() => _storage.GetAccessToken();
        public void SetAccessToken(string token) => _storage.SetAccessToken(token);
        public Task<string?> GetRefreshTokenAsync() => _storage.GetRefreshTokenAsync();
        public Task SetRefreshTokenAsync(string token) => _storage.SetRefreshTokenAsync(token);
        public Task ClearAsync() => _storage.ClearAsync();
    }
}
