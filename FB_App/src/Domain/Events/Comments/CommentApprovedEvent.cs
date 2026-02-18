namespace FB_App.Domain.Events.Comments;

public sealed class CommentApprovedEvent(Comment comment, string approvedBy) : BaseEvent
{
    public Comment Comment { get; } = comment;
    public string ApprovedBy { get; } = approvedBy;
}
