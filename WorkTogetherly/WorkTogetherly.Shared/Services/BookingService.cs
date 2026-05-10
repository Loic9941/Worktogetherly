using System.Net.Http.Json;
using System.Text.Json.Serialization;
using WorkTogetherly.Shared.Models;

namespace WorkTogetherly.Shared.Services
{
    public class BookingService
    {
        private readonly HttpClient _authClient;

        public BookingService(IHttpClientFactory httpClientFactory)
        {
            _authClient = httpClientFactory.CreateClient("Auth");
        }

        public async Task<List<BookingDto>> GetMyBookingsAsync(DateTime from, DateTime to)
        {
            var fromStr = from.ToString("o");
            var toStr = to.ToString("o");
            var result = await _authClient.GetFromJsonAsync<List<BookingDto>>(
                $"api/bookings/mine?from={Uri.EscapeDataString(fromStr)}&to={Uri.EscapeDataString(toStr)}");
            return result ?? [];
        }

        public async Task<(bool Success, string? Error)> CreateBookingAsync(int slotId, TimeOnly arrivalTime, List<int>? materialIds = null)
        {
            var response = await _authClient.PostAsJsonAsync("api/bookings", new { SlotId = slotId, ArrivalTime = arrivalTime, MaterialIds = materialIds ?? [] });
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

        public async Task<(bool Success, string? Error)> CancelBookingAsync(int bookingId)
        {
            var response = await _authClient.DeleteAsync($"api/bookings/{bookingId}");
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

        public async Task<(BookingDto? Booking, string? Error)> UpdateArrivalTimeAsync(int bookingId, TimeOnly arrivalTime)
        {
            var response = await _authClient.PatchAsJsonAsync($"api/bookings/{bookingId}/arrival-time", new { ArrivalTime = arrivalTime });
            if (response.IsSuccessStatusCode)
            {
                var booking = await response.Content.ReadFromJsonAsync<BookingDto>();
                return (booking, null);
            }

            try
            {
                var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
                return (null, problem?.Detail ?? problem?.Title ?? "Une erreur est survenue");
            }
            catch
            {
                return (null, "Une erreur est survenue");
            }
        }

        private record ProblemResponse(
            [property: JsonPropertyName("title")] string? Title,
            [property: JsonPropertyName("detail")] string? Detail);
    }
}
