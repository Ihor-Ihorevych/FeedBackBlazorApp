using FB_App.Application.Common.Extensions;
using FB_App.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FB_App.Web.Hubs;

/// <summary>
/// SignalR hub for real-time administrator notifications.
/// Only users with the Administrator role can connect to this hub.
/// </summary>
[Authorize(Roles = Roles.Administrator)]
public sealed class AdminNotificationHub(ILogger<AdminNotificationHub> logger) : Hub
{
    private readonly ILogger<AdminNotificationHub> _logger = logger;

    public override async Task OnConnectedAsync()
    {
        _logger.LogIfLevel(LogLevel.Information,
            "Admin {UserId} connected to AdminNotificationHub. ConnectionId: {ConnectionId}",
            Context.UserIdentifier!,
            Context.ConnectionId);

        await Groups.AddToGroupAsync(Context.ConnectionId, "Administrators");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogIfLevel(LogLevel.Information,
            "Admin {UserId} disconnected from AdminNotificationHub. ConnectionId: {ConnectionId}",
            Context.UserIdentifier!,
            Context.ConnectionId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Administrators");

        await base.OnDisconnectedAsync(exception);
    }
}
