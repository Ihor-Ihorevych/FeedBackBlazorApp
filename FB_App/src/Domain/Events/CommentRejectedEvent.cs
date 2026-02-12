using FB_App.Domain.Common;
using FB_App.Domain.Entities;

namespace FB_App.Domain.Events;

/// <summary>
/// Domain event raised when a comment is rejected by an administrator.
/// </summary>
public class CommentRejectedEvent : BaseEvent
{
    public CommentRejectedEvent(Comment comment, string rejectedBy)
    {
        Comment = comment;
        RejectedBy = rejectedBy;
    }

    public Comment Comment { get; }
    public string RejectedBy { get; }
}
