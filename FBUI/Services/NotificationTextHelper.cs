namespace FBUI.Services;

public interface INotificationTextHelper
{
    string GetTitle(AdminNotification notification);
    string GetMessage(AdminNotification notification);

    NotificationIconType GetIconType(AdminNotification notification);
}

public enum NotificationIconType
{
    Info,
    Comment,
    Success,
    Warning,
    Error
}

public static class NotificationTypes
{
    public const string NewComment = "NewComment";
    public const string CommentStatusChanged = "CommentStatusChanged";
    public const string CommentApproved = "CommentApproved";
    public const string CommentRejected = "CommentRejected";
    public const string MovieCreated = "MovieCreated";
    public const string MovieDeleted = "MovieDeleted";
}


public class NotificationTextHelper : INotificationTextHelper
{
    public string GetTitle(AdminNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        return notification.Type switch
        {
            NotificationTypes.NewComment => "New Comment Pending",
            NotificationTypes.CommentStatusChanged => "Comment Status Updated",
            NotificationTypes.CommentApproved => "Comment Approved",
            NotificationTypes.CommentRejected => "Comment Rejected",
            NotificationTypes.MovieCreated => "Movie Created",
            NotificationTypes.MovieDeleted => "Movie Deleted",
            _ => "Notification"
        };
    }

    public string GetMessage(AdminNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        return notification.Type switch
        {
            NotificationTypes.NewComment => FormatNewCommentMessage(notification),
            NotificationTypes.CommentStatusChanged => FormatStatusChangedMessage(notification),
            NotificationTypes.CommentApproved => FormatApprovedMessage(notification),
            NotificationTypes.CommentRejected => FormatRejectedMessage(notification),
            NotificationTypes.MovieCreated => FormatMovieCreatedMessage(notification),
            NotificationTypes.MovieDeleted => FormatMovieDeletedMessage(notification),
            _ => "New notification received"
        };
    }

    public NotificationIconType GetIconType(AdminNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        return notification.Type switch
        {
            NotificationTypes.NewComment => NotificationIconType.Comment,
            NotificationTypes.CommentApproved => NotificationIconType.Success,
            NotificationTypes.CommentRejected => NotificationIconType.Warning,
            NotificationTypes.CommentStatusChanged => NotificationIconType.Info,
            NotificationTypes.MovieCreated => NotificationIconType.Success,
            NotificationTypes.MovieDeleted => NotificationIconType.Warning,
            _ => NotificationIconType.Info
        };
    }

    private static string FormatNewCommentMessage(AdminNotification notification)
    {
        var movieTitle = notification.MovieTitle ?? "Unknown Movie";
        var preview = notification.CommentPreview ?? "No preview available";
        return $"On \"{movieTitle}\": {preview}";
    }

    private static string FormatStatusChangedMessage(AdminNotification notification)
    {
        var status = notification.NewStatus ?? "unknown";
        var reviewer = notification.ReviewedBy ?? "Unknown";
        return $"Comment marked as {status} by {reviewer}";
    }

    private static string FormatApprovedMessage(AdminNotification notification)
    {
        var reviewer = notification.ReviewedBy ?? "Unknown";
        return $"Comment approved by {reviewer}";
    }

    private static string FormatRejectedMessage(AdminNotification notification)
    {
        var reviewer = notification.ReviewedBy ?? "Unknown";
        return $"Comment rejected by {reviewer}";
    }

    private static string FormatMovieCreatedMessage(AdminNotification notification)
    {
        var movieTitle = notification.MovieTitle ?? "Unknown Movie";
        return $"\"{movieTitle}\" will appear in the list in about 30 seconds";
    }

    private static string FormatMovieDeletedMessage(AdminNotification notification)
    {
        return "Movie deleted, it will dissapear in about 30 secs";
    }
}
