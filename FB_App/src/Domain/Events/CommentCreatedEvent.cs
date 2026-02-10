using FB_App.Domain.Entities;

namespace FB_App.Domain.Events;

public class CommentCreatedEvent : BaseEvent
{
    public CommentCreatedEvent(Comment comment)
    {
        Comment = comment;
    }

    public Comment Comment { get; }
}
