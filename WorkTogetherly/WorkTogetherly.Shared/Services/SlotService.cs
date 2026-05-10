using System.Net.Http.Json;
using System.Text.Json.Serialization;
using WorkTogetherly.Shared.Models;

namespace WorkTogetherly.Shared.Services
{
    public class SlotService
    {
        private readonly HttpClient _authClient;

        public SlotService(IHttpClientFactory httpClientFactory)
        {
            _authClient = httpClientFactory.CreateClient("Auth");
        }

        public async Task<List<SlotDto>> GetSlotsByWorkspaceAsync(int workspaceId)
        {
            var result = await _authClient.GetFromJsonAsync<List<SlotDto>>(
                $"api/workspaces/{workspaceId}/slots");
            return result ?? [];
        }

        public async Task<(SlotDto? Result, string? Error)> CreateSlotAsync(int workspaceId, SlotFormModel model)
        {
            var request = BuildRequest(model);
            var response = await _authClient.PostAsJsonAsync($"api/workspaces/{workspaceId}/slots", request);
            if (!response.IsSuccessStatusCode)
                return (null, await ReadErrorAsync(response));

            var result = await response.Content.ReadFromJsonAsync<SlotDto>();
            return (result, null);
        }

        public async Task<(SlotDto? Result, string? Error)> UpdateSlotAsync(int workspaceId, int slotId, SlotFormModel model)
        {
            var request = BuildRequest(model);
            var response = await _authClient.PutAsJsonAsync($"api/workspaces/{workspaceId}/slots/{slotId}", request);
            if (!response.IsSuccessStatusCode)
                return (null, await ReadErrorAsync(response));

            var result = await response.Content.ReadFromJsonAsync<SlotDto>();
            return (result, null);
        }

        public async Task<string?> CancelSlotAsync(int workspaceId, int slotId)
        {
            var response = await _authClient.DeleteAsync($"api/workspaces/{workspaceId}/slots/{slotId}");
            if (!response.IsSuccessStatusCode)
                return await ReadErrorAsync(response);

            return null;
        }

        private static object BuildRequest(SlotFormModel model)
        {
            var date = model.Date ?? DateTime.Today;
            var startDateTime = date.Date + (model.StartTime ?? TimeSpan.Zero);
            var endDateTime = date.Date + (model.EndTime ?? TimeSpan.Zero);
            return new
            {
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                Capacity = model.Capacity
            };
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
