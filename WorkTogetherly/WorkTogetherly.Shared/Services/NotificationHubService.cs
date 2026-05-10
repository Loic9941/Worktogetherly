using Microsoft.AspNetCore.SignalR.Client;
using WorkTogetherly.Shared.Models;
using WorkTogetherly.Shared.Settings;

namespace WorkTogetherly.Shared.Services;

public class NotificationHubService : IAsyncDisposable
{
    private readonly TokenStorageService _tokenStorage;
    private readonly ApiSettings _apiSettings;
    private HubConnection? _connection;

    public event Action<MessageDto>? OnMessageReceived;

    public NotificationHubService(TokenStorageService tokenStorage, ApiSettings apiSettings)
    {
        _tokenStorage = tokenStorage;
        _apiSettings = apiSettings;
    }

    public async Task StartAsync()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"{_apiSettings.BaseUrl}/hubs/notifications", options =>
            {
                options.AccessTokenProvider = () =>
                    Task.FromResult(_tokenStorage.GetAccessToken());
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<MessageDto>("ReceiveMessage", msg => OnMessageReceived?.Invoke(msg));

        await _connection.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
