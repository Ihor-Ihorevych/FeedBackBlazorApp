namespace FB_App.Domain.Entities;

using FB_App.Domain.Entities.Values;
using FB_App.Domain.Enums;
using FB_App.Domain.Events.Comments;

public sealed class Comment : BaseAuditableEntity<CommentId>
{
    public override CommentId Id { get; } = CommentId.CreateNew();

    public MovieId MovieId { get; private set; } = null!;
    public Movie Movie { get; private set; } = null!;
    
    public string UserId { get; private set; } = string.Empty;
    
    public string Text { get; private set; } = string.Empty;
    
    public CommentStatus Status { get; private set; } = CommentStatus.Pending;
    
    public string? ReviewedBy { get; private set; }
    
    public DateTimeOffset? ReviewedAt { get; private set; }

    /// <summary>
    /// Default constructor for EF Core.
    /// Comments should only be created through the Movie aggregate.
    /// </summary>
    public Comment()
    {
    }

    /// <summary>
    /// Gets a value indicating whether the comment is approved.
    /// </summary>
    public bool IsApproved => Status == CommentStatus.Approved;

    /// <summary>
    /// Gets a value indicating whether the comment is pending review.
    /// </summary>
    public bool IsPending => Status == CommentStatus.Pending;

    /// <summary>
    /// Gets a value indicating whether the comment has been reviewed.
    /// </summary>
    public bool IsReviewed => ReviewedAt.HasValue && !string.IsNullOrEmpty(ReviewedBy);


    /// <summary>
    /// Factory method to create a new comment.
    /// This is called only by the Movie aggregate root.
    /// </summary>
    public static Comment Create(MovieId movieId, string userId, string text)
    {
        if (movieId == Guid.Empty)
            throw new ArgumentException("Movie ID must be greater than zero.", nameof(movieId));
        
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Comment text cannot be null or empty.", nameof(text));

        return new Comment
        {
            MovieId = movieId,
            UserId = userId,
            Text = text,
            Status = CommentStatus.Pending,
            ReviewedBy = null,
            ReviewedAt = null
        };
    }

    /// <summary>
    /// Approves the comment.
    /// </summary>
    /// <param name="reviewedBy">The identifier of the user approving the comment.</param>
    /// <exception cref="ArgumentException">Thrown when reviewedBy is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when comment is not in pending status.</exception>
    public void Approve(string reviewedBy)
    {
        if (string.IsNullOrWhiteSpace(reviewedBy))
            throw new ArgumentException("Reviewed by cannot be null or empty.", nameof(reviewedBy));

        if (!IsPending)
            throw new InvalidOperationException("Only pending comments can be approved.");

        Status = CommentStatus.Approved;
        ReviewedBy = reviewedBy;
        ReviewedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new CommentApprovedEvent(this, reviewedBy));
    }

    /// <summary>
    /// Rejects the comment.
    /// </summary>
    /// <param name="reviewedBy">The identifier of the user rejecting the comment.</param>
    /// <exception cref="ArgumentException">Thrown when reviewedBy is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when comment is not in pending status.</exception>
    public void Reject(string reviewedBy)
    {
        if (string.IsNullOrWhiteSpace(reviewedBy))
            throw new ArgumentException("Reviewed by cannot be null or empty.", nameof(reviewedBy));

        if (!IsPending)
            throw new InvalidOperationException("Only pending comments can be rejected.");

        Status = CommentStatus.Rejected;
        ReviewedBy = reviewedBy;
        ReviewedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new CommentRejectedEvent(this, reviewedBy));
    }

    /// <summary>
    /// Resets the comment back to pending status.
    /// Only allowed for reviewed comments.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when comment is already pending.</exception>
    public void ResetToPending()
    {
        if (IsPending)
            throw new InvalidOperationException("Comment is already in pending status.");

        Status = CommentStatus.Pending;
        ReviewedBy = null;
        ReviewedAt = null;
    }
}
