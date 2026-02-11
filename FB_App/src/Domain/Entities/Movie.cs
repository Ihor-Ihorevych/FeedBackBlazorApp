namespace FB_App.Domain.Entities;

using FB_App.Domain.Entities.Values;
using FB_App.Domain.Enums;

public sealed class Movie : BaseAuditableEntity<MovieId>
{
    public static Movie Create(string title, string? description = null, int? releaseYear = null, string? director = null, string? genre = null, string? posterUrl = null, double? rating = null)
    {

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty.", nameof(title));
        var movie = new Movie
        {
            Title = title,
            Description = description,
            ReleaseYear = releaseYear,
            Director = director,
            Genre = genre,
            PosterUrl = posterUrl,
            Rating = rating
        };

        movie.AddDomainEvent(new MovieCreatedEvent(movie));

        return movie;
    }


    private readonly HashSet<Comment> _comments = new();

    public override MovieId Id { get; } = MovieId.CreateNew();

    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public int? ReleaseYear { get; set; }
    
    public string? Director { get; set; }
    
    public string? Genre { get; set; }
    
    public string? PosterUrl { get; set; }
    
    public double? Rating { get; set; }
    
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    public int ApprovedCommentsCount => _comments.Count(c => c.IsApproved);
    public int PendingCommentsCount => _comments.Count(c => c.IsPending);
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

        var comment = Comment.Create(Id, userId, text);
        _comments.Add(comment);

        AddDomainEvent(new CommentCreatedEvent(comment));

        return comment;
    }

    /// <summary>
    /// Removes a comment from the movie.
    /// </summary>
    /// <param name="commentId">The ID of the comment to remove.</param>
    /// <returns>True if the comment was removed; otherwise false.</returns>
    public bool RemoveComment(CommentId commentId)
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
    public void ApproveComment(CommentId commentId, string reviewedBy)
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
    public void RejectComment(CommentId commentId, string reviewedBy)
    {
        var comment = GetCommentOrThrow(commentId);
        comment.Reject(reviewedBy);
    }

    /// <summary>
    /// Gets a comment by its ID.
    /// </summary>
    /// <param name="commentId">The ID of the comment.</param>
    /// <returns>The comment if found; otherwise null.</returns>
    public Comment? GetComment(CommentId commentId) => _comments.FirstOrDefault(c => c.Id == commentId);

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

    public bool HasPendingComments => PendingCommentsCount > 0;
    public int TotalCommentsCount => _comments.Count;

    /// <summary>
    ///  Retrieves the comment associated with the specified comment ID, or throws an exception if the comment does not
    /// exist.
    /// </summary>
    /// <param name="commentId">The identifier of the comment to retrieve. Cannot be null.</param>
    /// <returns>The comment associated with the specified comment ID.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a comment with the specified ID is not found in the movie aggregate.</exception>
    private Comment GetCommentOrThrow(CommentId commentId)
    {
        var comment = GetComment(commentId);
        return comment switch
        {
            null => throw new InvalidOperationException($"Comment with ID '{commentId}' not found in this movie aggregate."),
            _ => comment
        };
    }
}
