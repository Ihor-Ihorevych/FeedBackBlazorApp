namespace FB_App.Domain.Entities;

using FB_App.Domain.Entities.Values;
using FB_App.Domain.Enums;

/// <summary>
/// Movie aggregate root.
/// Manages the lifecycle and invariants of movie and its associated comments.
/// </summary>
public sealed class Movie : BaseAuditableEntity<int>
{
    private readonly List<Comment> _comments = new();

    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public int? ReleaseYear { get; set; }
    
    public string? Director { get; set; }
    
    public string? Genre { get; set; }
    
    public string? PosterUrl { get; set; }
    
    public double? Rating { get; set; }
    
    /// <summary>
    /// Gets the read-only collection of comments for this movie.
    /// Comments should only be modified through aggregate methods.
    /// </summary>
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    /// <summary>
    /// Gets the strongly-typed identifier for this movie.
    /// </summary>
    public MovieId MovieId => MovieId.Create(Id);

    #region Comment Management

    /// <summary>
    /// Gets the count of approved comments for this movie.
    /// </summary>
    public int ApprovedCommentsCount => _comments.Count(c => c.IsApproved);

    /// <summary>
    /// Gets the count of pending comments awaiting moderation.
    /// </summary>
    public int PendingCommentsCount => _comments.Count(c => c.IsPending);

    /// <summary>
    /// Gets the count of rejected comments.
    /// </summary>
    public int RejectedCommentsCount => _comments.Count(c => c.Status == CommentStatus.Rejected);

    /// <summary>
    /// Adds a new comment to the movie.
    /// </summary>
    /// <param name="userId">The user ID of the comment author.</param>
    /// <param name="text">The comment text.</param>
    /// <returns>The created comment.</returns>
    /// <exception cref="ArgumentException">Thrown when arguments are null or empty.</exception>
    public Comment AddComment(string userId, string text)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Comment text cannot be null or empty.", nameof(text));

        var comment = Comment.Create(this.Id, userId, text);
        _comments.Add(comment);

        return comment;
    }

    /// <summary>
    /// Removes a comment from the movie.
    /// </summary>
    /// <param name="commentId">The ID of the comment to remove.</param>
    /// <returns>True if the comment was removed; otherwise false.</returns>
    public bool RemoveComment(int commentId)
    {
        var comment = _comments.FirstOrDefault(c => c.Id == commentId);
        if (comment == null)
            return false;

        _comments.Remove(comment);
        return true;
    }

    /// <summary>
    /// Approves a pending comment.
    /// </summary>
    /// <param name="commentId">The ID of the comment to approve.</param>
    /// <param name="reviewedBy">The identifier of the user approving the comment.</param>
    /// <exception cref="InvalidOperationException">Thrown when comment is not found.</exception>
    /// <exception cref="ArgumentException">Thrown when reviewedBy is null or empty.</exception>
    public void ApproveComment(int commentId, string reviewedBy)
    {
        var comment = GetCommentOrThrow(commentId);
        comment.Approve(reviewedBy);
    }

    /// <summary>
    /// Rejects a pending comment.
    /// </summary>
    /// <param name="commentId">The ID of the comment to reject.</param>
    /// <param name="reviewedBy">The identifier of the user rejecting the comment.</param>
    /// <exception cref="InvalidOperationException">Thrown when comment is not found.</exception>
    /// <exception cref="ArgumentException">Thrown when reviewedBy is null or empty.</exception>
    public void RejectComment(int commentId, string reviewedBy)
    {
        var comment = GetCommentOrThrow(commentId);
        comment.Reject(reviewedBy);
    }

    /// <summary>
    /// Gets a comment by its ID.
    /// </summary>
    /// <param name="commentId">The ID of the comment.</param>
    /// <returns>The comment if found; otherwise null.</returns>
    public Comment? GetComment(int commentId) => _comments.FirstOrDefault(c => c.Id == commentId);

    /// <summary>
    /// Gets all approved comments for this movie.
    /// </summary>
    public IReadOnlyCollection<Comment> GetApprovedComments() => 
        _comments.Where(c => c.IsApproved).ToList().AsReadOnly();

    /// <summary>
    /// Gets all pending comments for this movie.
    /// </summary>
    public IReadOnlyCollection<Comment> GetPendingComments() =>
        _comments.Where(c => c.IsPending).ToList().AsReadOnly();

    /// <summary>
    /// Checks if the movie has any pending comments.
    /// </summary>
    public bool HasPendingComments() => PendingCommentsCount > 0;

    /// <summary>
    /// Gets the total number of comments for this movie.
    /// </summary>
    public int TotalCommentsCount => _comments.Count;

    private Comment GetCommentOrThrow(int commentId)
    {
        var comment = GetComment(commentId);
        if (comment == null)
            throw new InvalidOperationException($"Comment with ID '{commentId}' not found in this movie aggregate.");
        return comment;
    }

    #endregion
}
