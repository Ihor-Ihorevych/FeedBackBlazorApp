namespace FB_App.Domain.Events.Comments;

public sealed class CommentDeletedEvent(Comment comment) : BaseEvent
{
    public Comment Comment { get; } = comment;
}
