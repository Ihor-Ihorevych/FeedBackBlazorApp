using FB_App.Domain.Entities;

namespace FB_App.Domain.Events;

public class CommentRejectedEvent : BaseEvent
{
    public CommentRejectedEvent(Comment comment)
    {
        Comment = comment;
    }

    public Comment Comment { get; }
}
