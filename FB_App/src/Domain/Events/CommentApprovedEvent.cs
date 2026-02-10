using FB_App.Domain.Entities;

namespace FB_App.Domain.Events;

public class CommentApprovedEvent : BaseEvent
{
    public CommentApprovedEvent(Comment comment)
    {
        Comment = comment;
    }

    public Comment Comment { get; }
}
