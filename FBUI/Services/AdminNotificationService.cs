using FBUI.Configuration;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace FBUI.Services;

public static class ConnectionStates
{
    public const string Connected = "Connected";
    public const string Disconnected = "Disconnected";
    public const string Reconnecting = "Reconnecting";
    public const string Failed = "Failed";
    public const string Connecting = "Connecting";
}

public interface IAdminNotificationService : IAsyncDisposable
{
    string ConnectionState { get; }
    bool IsConnected { get; }

    event Action<string>? OnConnectionStateChanged;
    event Action<AdminNotification>? OnNotificationReceived;

    ValueTask DisposeAsync();
    string? GetCurrentToken();
    Task StartAsync();
    Task StopAsync();
}

public class AdminNotificationService : IAdminNotificationService
{
    private readonly ITokenStorageService _tokenStorage;
    private readonly ApiSettings _apiSettings;
    private HubConnection? _hubConnection;

    public event Action<AdminNotification>? OnNotificationReceived;
    public event Action<string>? OnConnectionStateChanged;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
    public string ConnectionState => _hubConnection?.State.ToString() ?? ConnectionStates.Disconnected;

    public AdminNotificationService(ITokenStorageService tokenStorage, IOptions<ApiSettings> apiSettings)
    {
        _tokenStorage = tokenStorage;
        _apiSettings = apiSettings.Value;
    }

    public async Task StartAsync()
    {
        if (_hubConnection is not null)
        {
            return;
        }

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_apiSettings.AdminNotificationsHubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_tokenStorage.AccessToken);
            })
            .WithAutomaticReconnect([TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)])
            .Build();

        _hubConnection.On<AdminNotification>("ReceiveNotification", notification =>
        {
            OnNotificationReceived?.Invoke(notification);
        });

        _hubConnection.Closed += async (error) =>
        {
            OnConnectionStateChanged?.Invoke(ConnectionStates.Disconnected);
            await Task.CompletedTask;
        };

        _hubConnection.Reconnecting += (error) =>
        {
            OnConnectionStateChanged?.Invoke(ConnectionStates.Reconnecting);
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += (connectionId) =>
        {
            OnConnectionStateChanged?.Invoke(ConnectionStates.Connected);
            return Task.CompletedTask;
        };

        try
        {
            await _hubConnection.StartAsync();
            OnConnectionStateChanged?.Invoke(ConnectionStates.Connected);
        }
        catch (Exception)
        {
            OnConnectionStateChanged?.Invoke(ConnectionStates.Failed);
        }
    }

    public async Task StopAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.StopAsync();
            OnConnectionStateChanged?.Invoke(ConnectionStates.Disconnected);
        }
    }

    public string? GetCurrentToken() => _tokenStorage.AccessToken;

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
    }
}

public record AdminNotification(
    string Type,
    Guid? MovieId,
    string? MovieTitle,
    Guid? CommentId,
    string? CommentPreview,
    string? UserId,
    string? NewStatus,
    string? ReviewedBy,
    DateTimeOffset Timestamp);
