namespace FB_App.Application.Common.Interfaces;

/// <summary>
/// Service for sending real-time notifications to administrators.
/// </summary>
public interface IAdminNotificationService
{
    /// <summary>
    /// Notifies all connected administrators about a new comment pending moderation.
    /// </summary>
    /// <param name="movieId">The ID of the movie the comment belongs to.</param>
    /// <param name="movieTitle">The title of the movie.</param>
    /// <param name="commentId">The ID of the new comment.</param>
    /// <param name="commentText">The text of the comment (preview).</param>
    /// <param name="userId">The ID of the user who created the comment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task NotifyNewCommentAsync(
        Guid movieId,
        string movieTitle,
        Guid commentId,
        string commentText,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies all connected administrators that a movie was created.
    /// </summary>
    /// <param name="movieId">The ID of the movie.</param>
    /// <param name="movieTitle">The title of the movie.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task NotifyMovieCreatedAsync(
        Guid movieId,
        string movieTitle,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies all connected administrators that a movie was deleted.
    /// </summary>
    /// <param name="movieId">The ID of the movie.</param>
    /// <param name="movieTitle">The title of the movie.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task NotifyMovieDeletedAsync(
        Guid movieId,
        string movieTitle,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies all connected administrators about a comment status change.
    /// </summary>
    /// <param name="commentId">The ID of the comment.</param>
    /// <param name="newStatus">The new status of the comment.</param>
    /// <param name="reviewedBy">The ID of the admin who reviewed the comment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task NotifyCommentStatusChangedAsync(
        Guid commentId,
        string newStatus,
        string reviewedBy,
        CancellationToken cancellationToken = default);
}
