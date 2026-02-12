using FB_App.Application.Common.Interfaces;
using FB_App.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FB_App.Web.Services;

/// <summary>
/// SignalR-based implementation of administrator notification service.
/// </summary>
public class AdminNotificationService : IAdminNotificationService
{
    private readonly IHubContext<AdminNotificationHub> _hubContext;
    private readonly ILogger<AdminNotificationService> _logger;

    public AdminNotificationService(
        IHubContext<AdminNotificationHub> hubContext,
        ILogger<AdminNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyNewCommentAsync(
        Guid movieId,
        string movieTitle,
        Guid commentId,
        string commentText,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending new comment notification to administrators. MovieId: {MovieId}, CommentId: {CommentId}",
            movieId,
            commentId);

        var notification = new
        {
            Type = "NewComment",
            MovieId = movieId,
            MovieTitle = movieTitle,
            CommentId = commentId,
            CommentPreview = commentText,
            UserId = userId,
            Timestamp = DateTimeOffset.UtcNow
        };

        await _hubContext.Clients
            .Group("Administrators")
            .SendAsync("ReceiveNotification", notification, cancellationToken);
    }

    public async Task NotifyCommentStatusChangedAsync(
        Guid commentId,
        string newStatus,
        string reviewedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending comment status change notification. CommentId: {CommentId}, NewStatus: {NewStatus}",
            commentId,
            newStatus);

        var notification = new
        {
            Type = "CommentStatusChanged",
            CommentId = commentId,
            NewStatus = newStatus,
            ReviewedBy = reviewedBy,
            Timestamp = DateTimeOffset.UtcNow
        };

        await _hubContext.Clients
            .Group("Administrators")
            .SendAsync("ReceiveNotification", notification, cancellationToken);
    }
}
