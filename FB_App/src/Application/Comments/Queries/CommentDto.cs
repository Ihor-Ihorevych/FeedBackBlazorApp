namespace FB_App.Application.Comments.Queries;

public class CommentDto
{
    public int Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public DateTimeOffset Created { get; init; }
}
