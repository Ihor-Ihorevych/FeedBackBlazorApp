namespace FB_App.Domain.Events;

public sealed class CommentRejectedEvent : BaseEvent
{
    public CommentRejectedEvent(Comment comment, string rejectedBy)
    {
        Comment = comment;
        RejectedBy = rejectedBy;
    }

    public Comment Comment { get; }
    public string RejectedBy { get; }
}
