using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace FBUI.Services;

/// <summary>
/// Service for managing SignalR connection to receive admin notifications.
/// </summary>
public class AdminNotificationService : IAsyncDisposable
{
    private readonly TokenStorageService _tokenStorage;
    private readonly string _hubUrl;
    private HubConnection? _hubConnection;

    public event Action<AdminNotification>? OnNotificationReceived;
    public event Action<string>? OnConnectionStateChanged;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
    public string ConnectionState => _hubConnection?.State.ToString() ?? "Disconnected";

    public AdminNotificationService(TokenStorageService tokenStorage, IConfiguration configuration)
    {
        _tokenStorage = tokenStorage;
        var apiBaseAddress = configuration["ApiBaseAddress"] ?? "https://localhost:5001";
        _hubUrl = $"{apiBaseAddress}/hubs/admin-notifications";
    }

    public async Task StartAsync()
    {
        if (_hubConnection is not null)
        {
            return;
        }

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_tokenStorage.AccessToken);
            })
            .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
            .Build();

        _hubConnection.On<AdminNotification>("ReceiveNotification", notification =>
        {
            OnNotificationReceived?.Invoke(notification);
        });

        _hubConnection.Closed += async (error) =>
        {
            OnConnectionStateChanged?.Invoke("Disconnected");
            await Task.CompletedTask;
        };

        _hubConnection.Reconnecting += (error) =>
        {
            OnConnectionStateChanged?.Invoke("Reconnecting");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += (connectionId) =>
        {
            OnConnectionStateChanged?.Invoke("Connected");
            return Task.CompletedTask;
        };

        try
        {
            await _hubConnection.StartAsync();
            OnConnectionStateChanged?.Invoke("Connected");
        }
        catch (Exception)
        {
            OnConnectionStateChanged?.Invoke("Failed");
        }
    }

    public async Task StopAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.StopAsync();
            OnConnectionStateChanged?.Invoke("Disconnected");
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
