namespace FB_App.Domain.Events;

public sealed class CommentCreatedEvent : BaseEvent
{
    public CommentCreatedEvent(Comment comment)
    {
        Comment = comment;
    }

    public Comment Comment { get; }
}
