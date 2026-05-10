using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using WorkTogetherly.Shared.Services;

namespace WorkTogetherly.Shared.Auth
{
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly TokenStorageService _tokenStorage;
        private readonly IAuthService _authService;
        private readonly NavigationManager _navigation;

        public AuthTokenHandler(TokenStorageService tokenStorage, IAuthService authService, NavigationManager navigation)
        {
            _tokenStorage = tokenStorage;
            _authService = authService;
            _navigation = navigation;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = _tokenStorage.GetAccessToken();
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshed = await _authService.TryRefreshTokenAsync();
                if (refreshed)
                {
                    var newToken = _tokenStorage.GetAccessToken();
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                    response = await base.SendAsync(request, cancellationToken);
                }
                else
                {
                    _navigation.NavigateTo("/login");
                }
            }

            return response;
        }
    }
}
