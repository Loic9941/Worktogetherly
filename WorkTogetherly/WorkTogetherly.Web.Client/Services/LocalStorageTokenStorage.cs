using Microsoft.JSInterop;
using WorkTogetherly.Shared.Services;

namespace WorkTogetherly.Web.Client.Services
{
    public class LocalStorageTokenStorage : ITokenStorage
    {
        private readonly IJSRuntime _js;
        private string? _accessToken;

        public LocalStorageTokenStorage(IJSRuntime js)
        {
            _js = js;
        }

        public string? GetAccessToken() => _accessToken;

        public void SetAccessToken(string token)
        {
            _accessToken = token;
            _ = _js.InvokeVoidAsync("sessionStorage.setItem", "access_token", token);
        }

        public async Task<string?> GetRefreshTokenAsync()
            => await _js.InvokeAsync<string?>("localStorage.getItem", "refresh_token");

        public async Task SetRefreshTokenAsync(string token)
            => await _js.InvokeVoidAsync("localStorage.setItem", "refresh_token", token);

        public async Task ClearAsync()
        {
            _accessToken = null;
            await _js.InvokeVoidAsync("sessionStorage.removeItem", "access_token");
            await _js.InvokeVoidAsync("localStorage.removeItem", "refresh_token");
        }
    }
}
