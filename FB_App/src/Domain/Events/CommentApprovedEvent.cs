namespace FB_App.Domain.Events;

public sealed class CommentApprovedEvent : BaseEvent
{
    public CommentApprovedEvent(Comment comment, string approvedBy)
    {
        Comment = comment;
        ApprovedBy = approvedBy;
    }

    public Comment Comment { get; }
    public string ApprovedBy { get; }
}
