namespace FB_App.Domain.Events.Comments;

public sealed class CommentCreatedEvent : BaseEvent
{
    public CommentCreatedEvent(Comment comment)
    {
        Comment = comment;
    }

    public Comment Comment { get; }
}
