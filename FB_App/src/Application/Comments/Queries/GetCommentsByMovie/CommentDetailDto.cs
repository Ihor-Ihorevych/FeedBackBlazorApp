using FB_App.Domain.Enums;

namespace FB_App.Application.Comments.Queries.GetCommentsByMovie;

public class CommentDetailDto
{
    public Guid Id { get; init; }
    public Guid MovieId { get; init; }
    public string Text { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public CommentStatus Status { get; init; }
    public DateTimeOffset Created { get; init; }
    public string? ReviewedBy { get; init; }
    public DateTimeOffset? ReviewedAt { get; init; }
}
