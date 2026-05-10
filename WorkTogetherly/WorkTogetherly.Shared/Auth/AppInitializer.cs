using WorkTogetherly.Shared.Services;

namespace WorkTogetherly.Shared.Auth
{
    public class AppInitializer : IAppInitializer
    {
        private readonly IAuthService _authService;
        private readonly TokenStorageService _tokenStorage;

        public AppInitializer(IAuthService authService, TokenStorageService tokenStorage)
        {
            _authService = authService;
            _tokenStorage = tokenStorage;
        }

        public async Task InitializeAsync()
        {
            if (string.IsNullOrEmpty(_tokenStorage.GetAccessToken()))
                await _authService.TryRefreshTokenAsync();
        }
    }
}
