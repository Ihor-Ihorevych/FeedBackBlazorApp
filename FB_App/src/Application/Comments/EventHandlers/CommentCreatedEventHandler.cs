using FB_App.Application.Common.Interfaces;
using FB_App.Domain.Events.Comments;
using Microsoft.Extensions.Logging;

namespace FB_App.Application.Comments.EventHandlers;


public class CommentCreatedEventHandler : INotificationHandler<CommentCreatedEvent>
{
    private readonly ILogger<CommentCreatedEventHandler> _logger;
    private readonly IAdminNotificationService _adminNotificationService;
    private readonly IApplicationDbContext _context;

    public CommentCreatedEventHandler(
        ILogger<CommentCreatedEventHandler> logger,
        IAdminNotificationService adminNotificationService,
        IApplicationDbContext context)
    {
        _logger = logger;
        _adminNotificationService = adminNotificationService;
        _context = context;
    }

    public async Task Handle(CommentCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Domain Event: {DomainEvent} - Comment {CommentId} created for Movie {MovieId} by User {UserId}",
            notification.GetType().Name,
            notification.Comment.Id,
            notification.Comment.MovieId,
            notification.Comment.UserId);

        var movie = await _context.Movies
            .FirstOrDefaultAsync(m => m.Id == notification.Comment.MovieId, cancellationToken);

        var movieTitle = movie?.Title ?? "Unknown Movie";

        await _adminNotificationService.NotifyNewCommentAsync(
            notification.Comment.MovieId,
            movieTitle,
            notification.Comment.Id,
            notification.Comment.Text.Length > 100 
                ? notification.Comment.Text[..100] + "..." 
                : notification.Comment.Text,
            notification.Comment.UserId,
            cancellationToken);
    }
}
