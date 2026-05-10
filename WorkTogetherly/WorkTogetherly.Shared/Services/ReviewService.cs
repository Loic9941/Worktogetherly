using System.Net.Http.Json;
using System.Text.Json.Serialization;
using WorkTogetherly.Shared.Models;

namespace WorkTogetherly.Shared.Services
{
    public class ReviewService
    {
        private readonly HttpClient _authClient;

        public ReviewService(IHttpClientFactory httpClientFactory)
        {
            _authClient = httpClientFactory.CreateClient("Auth");
        }

        public async Task<(ReviewDto? Review, string? Error)> CreateAsync(CreateReviewRequest request)
        {
            var response = await _authClient.PostAsJsonAsync("api/reviews", request);
            if (response.IsSuccessStatusCode)
            {
                var review = await response.Content.ReadFromJsonAsync<ReviewDto>();
                return (review, null);
            }

            return (null, await ReadErrorAsync(response));
        }

        public async Task<(ReviewDto? Review, string? Error)> UpdateAsync(int reviewId, UpdateReviewRequest request)
        {
            var response = await _authClient.PutAsJsonAsync($"api/reviews/{reviewId}", request);
            if (response.IsSuccessStatusCode)
            {
                var review = await response.Content.ReadFromJsonAsync<ReviewDto>();
                return (review, null);
            }

            return (null, await ReadErrorAsync(response));
        }

        public async Task<ReviewDto?> GetByBookingAsync(int bookingId)
        {
            try
            {
                return await _authClient.GetFromJsonAsync<ReviewDto>($"api/reviews/booking/{bookingId}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<ReviewDto>> GetByWorkspaceAsync(int workspaceId)
        {
            var result = await _authClient.GetFromJsonAsync<List<ReviewDto>>($"api/workspaces/{workspaceId}/reviews");
            return result ?? [];
        }

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

        private record ProblemResponse(
            [property: JsonPropertyName("title")] string? Title,
            [property: JsonPropertyName("detail")] string? Detail);
    }
}
