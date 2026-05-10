using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Forms;
using WorkTogetherly.Shared.Models;

namespace WorkTogetherly.Shared.Services
{
    public class UserService
    {
        private readonly HttpClient _authClient;
        private readonly HttpClient _publicClient;

        public UserService(IHttpClientFactory httpClientFactory)
        {
            _authClient = httpClientFactory.CreateClient("Auth");
            _publicClient = httpClientFactory.CreateClient("Public");
        }

        public async Task<UserProfileDto?> GetProfileAsync()
        {
            return await _authClient.GetFromJsonAsync<UserProfileDto>("api/auth/me");
        }

        public async Task<(bool Success, string? Error)> UpdateProfileAsync(UpdateProfileDto dto)
        {
            var response = await _authClient.PutAsJsonAsync("api/auth/me", dto);
            if (response.IsSuccessStatusCode) return (true, null);

            try
            {
                var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
                return (false, problem?.Detail ?? problem?.Title ?? "Une erreur est survenue");
            }
            catch
            {
                return (false, "Une erreur est survenue");
            }
        }

        public async Task<(bool Success, string? Error)> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            var response = await _authClient.PutAsJsonAsync("api/auth/me/password", new { currentPassword, newPassword });
            if (response.IsSuccessStatusCode) return (true, null);

            try
            {
                var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
                return (false, problem?.Detail ?? problem?.Title ?? "Une erreur est survenue");
            }
            catch
            {
                return (false, "Une erreur est survenue");
            }
        }

        public async Task<(UserProfileDto? Result, string? Error)> UploadPhotoAsync(IBrowserFile file)
        {
            using var content = new MultipartFormDataContent();
            var stream = file.OpenReadStream(5 * 1024 * 1024);
            using var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.Name);

            var response = await _authClient.PostAsync("api/auth/me/photo", content);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UserProfileDto>();
                return (result, null);
            }

            try
            {
                var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
                return (null, problem?.Detail ?? problem?.Title ?? "Erreur lors du téléversement");
            }
            catch { return (null, "Erreur lors du téléversement"); }
        }

        public async Task<(UserProfileDto? Result, string? Error)> DeletePhotoAsync()
        {
            var response = await _authClient.DeleteAsync("api/auth/me/photo");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UserProfileDto>();
                return (result, null);
            }

            try
            {
                var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
                return (null, problem?.Detail ?? problem?.Title ?? "Erreur lors de la suppression");
            }
            catch { return (null, "Erreur lors de la suppression"); }
        }

        public async Task<(bool Success, string? Error)> ForgotPasswordAsync(string email)
        {
            var response = await _publicClient.PostAsJsonAsync("api/auth/forgot-password", new { email });
            if (response.IsSuccessStatusCode) return (true, null);

            try
            {
                var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
                return (false, problem?.Detail ?? problem?.Title ?? "Une erreur est survenue");
            }
            catch { return (false, "Une erreur est survenue"); }
        }

        public async Task<(bool Success, string? Error)> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var response = await _publicClient.PostAsJsonAsync("api/auth/reset-password", new { email, token, newPassword });
            if (response.IsSuccessStatusCode) return (true, null);

            try
            {
                var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
                return (false, problem?.Detail ?? problem?.Title ?? "Une erreur est survenue");
            }
            catch { return (false, "Une erreur est survenue"); }
        }

        public string GetPhotoUrl(string? photoPath)
        {
            if (string.IsNullOrEmpty(photoPath)) return string.Empty;
            var baseAddress = _authClient.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;
            return baseAddress + photoPath;
        }

        private record ProblemResponse(
            [property: JsonPropertyName("title")] string? Title,
            [property: JsonPropertyName("detail")] string? Detail);
    }
}
