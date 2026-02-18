namespace FB_App.Domain.Events.Comments;

public sealed class CommentCreatedEvent(Comment comment) : BaseEvent
{
    public Comment Comment { get; } = comment;
}
