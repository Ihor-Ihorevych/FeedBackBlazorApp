using FB_App.Application.Common.Interfaces;
using FB_App.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FB_App.Web.Services;


public class AdminNotificationService : IAdminNotificationService
{
    private const string AdminGroupName_ = "Administrators";
    private const string RecieveNotification_ = "ReceiveNotification";
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
            .Group(AdminGroupName_)
            .SendAsync(RecieveNotification_, notification, cancellationToken);
    }

    public async Task NotifyMovieCreatedAsync(
        Guid movieId,
        string movieTitle,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending movie created notification to administrators. MovieId: {MovieId}",
            movieId);

        var notification = new
        {
            Type = "MovieCreated",
            MovieId = movieId,
            MovieTitle = movieTitle,
            Timestamp = DateTimeOffset.UtcNow
        };

        await _hubContext.Clients
            .Group(AdminGroupName_)
            .SendAsync(RecieveNotification_, notification, cancellationToken);
    }

    public async Task NotifyMovieDeletedAsync(
        Guid movieId,
        string movieTitle,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending movie deleted notification to administrators. MovieId: {MovieId}",
            movieId);

        var notification = new
        {
            Type = "MovieDeleted",
            MovieId = movieId,
            MovieTitle = movieTitle,
            Timestamp = DateTimeOffset.UtcNow
        };

        await _hubContext.Clients
            .Group(AdminGroupName_)
            .SendAsync(RecieveNotification_, notification, cancellationToken);
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
            .Group(AdminGroupName_)
            .SendAsync(RecieveNotification_, notification, cancellationToken);
    }
}
