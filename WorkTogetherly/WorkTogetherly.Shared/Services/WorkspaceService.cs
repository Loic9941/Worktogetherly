using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using WorkTogetherly.Shared.Models;

namespace WorkTogetherly.Shared.Services
{
    public class WorkspaceService
    {
        private readonly HttpClient _authClient;
        private readonly HttpClient _geocodingClient;

        public WorkspaceService(IHttpClientFactory httpClientFactory)
        {
            _authClient = httpClientFactory.CreateClient("Auth");
            _geocodingClient = httpClientFactory.CreateClient("Geocoding");
        }

        public async Task<WorkspaceDto?> GetMyWorkspaceAsync()
        {
            var response = await _authClient.GetAsync("api/workspaces/mine");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<WorkspaceDto>();
        }

        public async Task<(WorkspaceDto? Result, string? Error)> CreateAsync(WorkspaceFormModel model)
        {
            var response = await _authClient.PostAsJsonAsync("api/workspaces", BuildRequest(model));
            if (!response.IsSuccessStatusCode)
                return (null, await ReadErrorAsync(response));

            var result = await response.Content.ReadFromJsonAsync<WorkspaceDto>();
            return (result, null);
        }

        public async Task<(WorkspaceDto? Result, string? Error)> UpdateAsync(int id, WorkspaceFormModel model)
        {
            var response = await _authClient.PutAsJsonAsync($"api/workspaces/{id}", BuildRequest(model));
            if (!response.IsSuccessStatusCode)
                return (null, await ReadErrorAsync(response));

            var result = await response.Content.ReadFromJsonAsync<WorkspaceDto>();
            return (result, null);
        }

        public async Task<List<WorkspaceSearchResultDto>> SearchWorkspacesAsync(WorkspaceSearchRequest request)
        {
            var url = $"api/workspaces/search?latitude={request.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                      $"&longitude={request.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                      $"&radiusKm={request.RadiusKm.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                      $"&date={request.Date:yyyy-MM-dd}";

            var response = await _authClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return [];
            return await response.Content.ReadFromJsonAsync<List<WorkspaceSearchResultDto>>() ?? [];
        }

        public async Task<WorkspaceDetailDto?> GetWorkspaceDetailsAsync(int id, DateOnly date)
        {
            var response = await _authClient.GetAsync($"api/workspaces/{id}/details?date={date:yyyy-MM-dd}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<WorkspaceDetailDto>();
        }

        public async Task<List<CitySuggestion>> GetCitySuggestionsAsync(string input, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length < 3) return [];
            try
            {
                var encoded = Uri.EscapeDataString(input);
                var response = await _geocodingClient.GetFromJsonAsync<PhotonResponse>(
                    $"api/?q={encoded}&limit=5&lang=fr", ct);

                if (response?.Features is null) return [];
                return response.Features
                    .Select(f => new CitySuggestion(
                        BuildDisplayName(f.Properties),
                        f.Geometry.Coordinates[1],
                        f.Geometry.Coordinates[0]))
                    .Where(s => !string.IsNullOrEmpty(s.DisplayName))
                    .ToList();
            }
            catch (OperationCanceledException) { return []; }
            catch { return []; }
        }

        public async Task<CitySuggestion?> ReverseGeocodeAsync(double latitude, double longitude)
        {
            try
            {
                var lat = latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var lon = longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var response = await _geocodingClient.GetFromJsonAsync<PhotonResponse>(
                    $"reverse?lat={lat}&lon={lon}");

                var feature = response?.Features?.FirstOrDefault();
                if (feature is null) return null;

                return new CitySuggestion(
                    BuildDisplayName(feature.Properties),
                    feature.Geometry.Coordinates[1],
                    feature.Geometry.Coordinates[0]);
            }
            catch { return null; }
        }

        public async Task<(double Lat, double Lon)?> GeocodeAddressAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) return null;

            try
            {
                var encoded = Uri.EscapeDataString(address);
                var response = await _geocodingClient.GetFromJsonAsync<PhotonResponse>(
                    $"api/?q={encoded}&limit=1&lang=fr");

                var feature = response?.Features?.FirstOrDefault();
                if (feature is null) return null;

                return (feature.Geometry.Coordinates[1], feature.Geometry.Coordinates[0]);
            }
            catch { return null; }
        }

        private static string BuildDisplayName(PhotonProperties p)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(p.Name)) parts.Add(p.Name);
            var street = string.Join(" ", new[] { p.Street, p.Housenumber }.Where(s => !string.IsNullOrEmpty(s)));
            if (!string.IsNullOrEmpty(street)) parts.Add(street);
            if (!string.IsNullOrEmpty(p.City)) parts.Add(p.City);
            if (!string.IsNullOrEmpty(p.Postcode)) parts.Add(p.Postcode);
            if (!string.IsNullOrEmpty(p.Country)) parts.Add(p.Country);
            return string.Join(", ", parts);
        }

        public string GetPhotoUrl(string? photoPath)
        {
            if (string.IsNullOrEmpty(photoPath)) return string.Empty;
            var baseAddress = _authClient.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;
            return baseAddress + photoPath;
        }

        public async Task<(WorkspaceDto? Result, string? Error)> UploadPhotoAsync(int workspaceId, IBrowserFile file)
        {
            await using var stream = file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.Name);

            var response = await _authClient.PostAsync($"api/workspaces/{workspaceId}/photo", content);
            if (!response.IsSuccessStatusCode)
                return (null, await ReadErrorAsync(response));

            var result = await response.Content.ReadFromJsonAsync<WorkspaceDto>();
            return (result, null);
        }

        public async Task<(WorkspaceDto? Result, string? Error)> DeletePhotoAsync(int workspaceId)
        {
            var response = await _authClient.DeleteAsync($"api/workspaces/{workspaceId}/photo");
            if (!response.IsSuccessStatusCode)
                return (null, await ReadErrorAsync(response));

            var result = await response.Content.ReadFromJsonAsync<WorkspaceDto>();
            return (result, null);
        }

        private static object BuildRequest(WorkspaceFormModel model) => new
        {
            model.Name,
            model.Description,
            model.Address,
            model.Latitude,
            model.Longitude,
            model.Capacity,
            model.IsActive,
            Materials = model.Materials
                .Where(m => m.Enabled)
                .Select(m => new { m.MaterialId, m.Quantity })
                .ToList(),
            RuleIds = model.Rules
                .Where(r => r.Enabled)
                .Select(r => r.RuleId)
                .ToList()
        };

        private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
        {
            try
            {
                var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
                return problem?.Detail ?? problem?.Title ?? "Une erreur est survenue";
            }
            catch
            {
                return "Une erreur est survenue";
            }
        }

        private record PhotonResponse(
            [property: JsonPropertyName("features")] PhotonFeature[]? Features);

        private record PhotonFeature(
            [property: JsonPropertyName("geometry")] PhotonGeometry Geometry,
            [property: JsonPropertyName("properties")] PhotonProperties Properties);

        private record PhotonGeometry(
            [property: JsonPropertyName("coordinates")] double[] Coordinates);

        private record PhotonProperties(
            [property: JsonPropertyName("name")] string? Name,
            [property: JsonPropertyName("street")] string? Street,
            [property: JsonPropertyName("housenumber")] string? Housenumber,
            [property: JsonPropertyName("city")] string? City,
            [property: JsonPropertyName("postcode")] string? Postcode,
            [property: JsonPropertyName("country")] string? Country);

        private record ProblemResponse(
            [property: JsonPropertyName("title")] string? Title,
            [property: JsonPropertyName("detail")] string? Detail);
    }
}
