using System.Net.Http.Headers;
using System.Net.Http.Json;
using WorkTogetherly.Shared.Auth;
using WorkTogetherly.Shared.Models;

namespace WorkTogetherly.Shared.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _publicClient;
        private readonly TokenStorageService _tokenStorage;
        private readonly JwtAuthenticationStateProvider _authStateProvider;

        public AuthService(
            IHttpClientFactory httpClientFactory,
            TokenStorageService tokenStorage,
            JwtAuthenticationStateProvider authStateProvider)
        {
            _publicClient = httpClientFactory.CreateClient("Public");
            _tokenStorage = tokenStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var response = await _publicClient.PostAsJsonAsync("api/auth/login", new { email, password });
            if (!response.IsSuccessStatusCode) return false;

            var result = await response.Content.ReadFromJsonAsync<AuthResult>();
            if (result is null) return false;

            await StoreTokensAsync(result);
            return true;
        }

        public async Task<bool> RegisterAsync(
            string email, string password, string firstName, string lastName)
        {
            var response = await _publicClient.PostAsJsonAsync("api/auth/register", new
            {
                email,
                password,
                firstName,
                lastName
            });
            if (!response.IsSuccessStatusCode) return false;

            var result = await response.Content.ReadFromJsonAsync<AuthResult>();
            if (result is null) return false;

            await StoreTokensAsync(result);
            return true;
        }

        public async Task LogoutAsync()
        {
            var accessToken = _tokenStorage.GetAccessToken();
            var refreshToken = await _tokenStorage.GetRefreshTokenAsync();

            using var request = string.IsNullOrEmpty(refreshToken)
                ? new HttpRequestMessage(HttpMethod.Post, "api/auth/logout")
                : new HttpRequestMessage(HttpMethod.Post, "api/auth/logout")
                  {
                      Content = JsonContent.Create(new { refreshToken })
                  };

            if (!string.IsNullOrEmpty(accessToken))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            await _publicClient.SendAsync(request);

            await _tokenStorage.ClearAsync();
            _authStateProvider.NotifyAuthenticationStateChanged();
        }

        // SemaphoreSlim(1,1) so only one refresh call hits the backend at a time.
        // Multiple 401 retries can arrive concurrently; the first one refreshes, the others just read the new token.
        private SemaphoreSlim _refreshLock = new(1, 1);
        private bool _isRefreshing = false;

        public async Task<bool> TryRefreshTokenAsync()
        {
            var refreshToken = await _tokenStorage.GetRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken))
            {
                await _tokenStorage.ClearAsync();
                _authStateProvider.NotifyAuthenticationStateChanged();
                return false;
            }

            await _refreshLock.WaitAsync();
            try
            {
                // A concurrent call already refreshed the token — return whatever is in storage now.
                if (_isRefreshing)
                    return !string.IsNullOrEmpty(_tokenStorage.GetAccessToken());

                _isRefreshing = true;
                HttpResponseMessage response;
                try
                {
                    response = await _publicClient.PostAsJsonAsync("api/auth/refresh", new { refreshToken });
                }
                catch (Exception)
                {
                    // Network error: keep the tokens intact so the user isn't logged out on a flaky connection.
                    return false;
                }

                if (!response.IsSuccessStatusCode)
                {
                    await _tokenStorage.ClearAsync();
                    _authStateProvider.NotifyAuthenticationStateChanged();
                    return false;
                }

                var result = await response.Content.ReadFromJsonAsync<AuthResult>();
                if (result is null)
                {
                    await _tokenStorage.ClearAsync();
                    _authStateProvider.NotifyAuthenticationStateChanged();
                    return false;
                }

                await StoreTokensAsync(result);
                return true;
            }
            finally
            {
                _isRefreshing = false;
                _refreshLock.Release();
            }
        }

        private async Task StoreTokensAsync(AuthResult result)
        {
            _tokenStorage.SetAccessToken(result.AccessToken);
            await _tokenStorage.SetRefreshTokenAsync(result.RefreshToken);
            _authStateProvider.NotifyAuthenticationStateChanged();
        }
    }
}
