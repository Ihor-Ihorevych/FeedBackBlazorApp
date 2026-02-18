namespace FB_App.Domain.Events.Comments;

public sealed class CommentRejectedEvent(Comment comment, string rejectedBy) : BaseEvent
{
    public Comment Comment { get; } = comment;
    public string RejectedBy { get; } = rejectedBy;
}
