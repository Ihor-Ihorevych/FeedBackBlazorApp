using FB_App.Domain.Common;
using FB_App.Domain.Entities;

namespace FB_App.Domain.Events;

/// <summary>
/// Domain event raised when a comment is approved by an administrator.
/// </summary>
public class CommentApprovedEvent : BaseEvent
{
    public CommentApprovedEvent(Comment comment, string approvedBy)
    {
        Comment = comment;
        ApprovedBy = approvedBy;
    }

    public Comment Comment { get; }
    public string ApprovedBy { get; }
}
