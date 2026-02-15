namespace FB_App.Domain.Events.Comments;

public sealed class CommentDeletedEvent : BaseEvent
{
    public CommentDeletedEvent(Comment comment)
    {
        Comment = comment;
    }
    public Comment Comment { get; }
}
