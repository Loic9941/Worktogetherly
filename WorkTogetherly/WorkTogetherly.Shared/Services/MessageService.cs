using System.Net.Http.Json;
using WorkTogetherly.Shared.Models;

namespace WorkTogetherly.Shared.Services;

public class MessageService
{
    private readonly HttpClient _authClient;

    public MessageService(IHttpClientFactory httpClientFactory)
    {
        _authClient = httpClientFactory.CreateClient("Auth");
    }

    public async Task<List<MessageDto>> GetMyMessagesAsync()
    {
        try
        {
            return await _authClient.GetFromJsonAsync<List<MessageDto>>("api/messages/mine") ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<bool> MarkAsReadAsync(int messageId)
    {
        var response = await _authClient.PatchAsync($"api/messages/{messageId}/read", null);
        return response.IsSuccessStatusCode;
    }

}
