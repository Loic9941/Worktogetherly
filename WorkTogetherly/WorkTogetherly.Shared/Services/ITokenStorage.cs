namespace WorkTogetherly.Shared.Services
{
    public interface ITokenStorage
    {
        string? GetAccessToken();
        void SetAccessToken(string token);
        Task<string?> GetRefreshTokenAsync();
        Task SetRefreshTokenAsync(string token);
        Task ClearAsync();
    }
}
