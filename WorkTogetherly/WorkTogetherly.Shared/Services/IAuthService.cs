namespace WorkTogetherly.Shared.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string email, string password);
        Task<bool> RegisterAsync(string email, string password, string firstName, string lastName);
        Task LogoutAsync();
        Task<bool> TryRefreshTokenAsync();
    }
}
