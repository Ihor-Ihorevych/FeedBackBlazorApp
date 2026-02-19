using FBUI.Services;

namespace FBUI.Tests;

public sealed class NotificationTextHelperTests
{
    private INotificationTextHelper _textHelper = null!;

    [SetUp]
    public void SetUp()
    {
        _textHelper = new NotificationTextHelper();
    }

    [TearDown]
    public void TearDown() 
    { 
        _textHelper = null!;
    }

    [Test]
    public void GetTitle_KnownTypes_ReturnsExpected()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_textHelper.GetTitle(new AdminNotification(NotificationTypes.NewComment, null, null, null, null, null, null, null, DateTimeOffset.Now)), Is.EqualTo("New Comment Pending"));
            Assert.That(_textHelper.GetTitle(new AdminNotification(NotificationTypes.CommentStatusChanged, null, null, null, null, null, null, null, DateTimeOffset.Now)), Is.EqualTo("Comment Status Updated"));
            Assert.That(_textHelper.GetTitle(new AdminNotification(NotificationTypes.CommentApproved, null, null, null, null, null, null, null, DateTimeOffset.Now)), Is.EqualTo("Comment Approved"));
            Assert.That(_textHelper.GetTitle(new AdminNotification(NotificationTypes.CommentRejected, null, null, null, null, null, null, null, DateTimeOffset.Now)), Is.EqualTo("Comment Rejected"));
            Assert.That(_textHelper.GetTitle(new AdminNotification(NotificationTypes.MovieCreated, null, null, null, null, null, null, null, DateTimeOffset.Now)), Is.EqualTo("Movie Created"));
            Assert.That(_textHelper.GetTitle(new AdminNotification(NotificationTypes.MovieDeleted, null, null, null, null, null, null, null, DateTimeOffset.Now)), Is.EqualTo("Movie Deleted"));
        }
    }

    [Test]
    public void GetMessage_NewComment_FormatsCorrectly()
    {
        var n = new AdminNotification(NotificationTypes.NewComment, Guid.NewGuid(), "My Movie", Guid.NewGuid(), "Nice comment", null, null, null, DateTimeOffset.Now);
        var msg = _textHelper.GetMessage(n);
        Assert.That(msg, Is.EqualTo("On \"My Movie\": Nice comment"));
    }

    [Test]
    public void GetMessage_NewComment_HandlesNulls()
    {
        var n = new AdminNotification(NotificationTypes.NewComment, null, null, null, null, null, null, null, DateTimeOffset.Now);
        var msg = _textHelper.GetMessage(n);
        Assert.That(msg, Is.EqualTo("On \"Unknown Movie\": No preview available"));
    }

    [Test]
    public void GetMessage_StatusChanged_FormatsCorrectly()
    {
        var n = new AdminNotification(NotificationTypes.CommentStatusChanged, null, null, null, null, null, "Reviewed", "Admin", DateTimeOffset.Now);
        var msg = _textHelper.GetMessage(n);
        Assert.That(msg, Is.EqualTo("Comment marked as Reviewed by Admin"));
    }

    [Test]
    public void GetMessage_ApprovedRejected_FormatsCorrectly()
    {
        var a = new AdminNotification(NotificationTypes.CommentApproved, null, null, null, null, null, null, "Reviewer", DateTimeOffset.Now);
        Assert.That(_textHelper.GetMessage(a), Is.EqualTo("Comment approved by Reviewer"));

        var r = new AdminNotification(NotificationTypes.CommentRejected, null, null, null, null, null, null, "Reviewer", DateTimeOffset.Now);
        Assert.That(_textHelper.GetMessage(r), Is.EqualTo("Comment rejected by Reviewer"));
    }

    [Test]
    public void GetMessage_MovieCreatedAndDeleted_FormatsCorrectly()
    {
        var c = new AdminNotification(NotificationTypes.MovieCreated, Guid.NewGuid(), "New Movie", null, null, null, null, null, DateTimeOffset.Now);
        Assert.That(_textHelper.GetMessage(c), Is.EqualTo("\"New Movie\" will appear in the list in about 30 seconds"));

        var d = new AdminNotification(NotificationTypes.MovieDeleted, Guid.NewGuid(), null, null, null, null, null, null, DateTimeOffset.Now);
        Assert.That(_textHelper.GetMessage(d), Is.EqualTo("Movie deleted, it will dissapear in about 30 secs"));
    }

    [Test]
    public void GetIconType_Mappings_AreCorrect()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_textHelper.GetIconType(new AdminNotification(NotificationTypes.NewComment, null, null, null, null, null, null, null, DateTimeOffset.Now)), Is.EqualTo(NotificationIconType.Comment));
            Assert.That(_textHelper.GetIconType(new AdminNotification(NotificationTypes.CommentApproved, null, null, null, null, null, null, null, DateTimeOffset.Now)), Is.EqualTo(NotificationIconType.Success));
            Assert.That(_textHelper.GetIconType(new AdminNotification(NotificationTypes.CommentRejected, null, null, null, null, null, null, null, DateTimeOffset.Now)), Is.EqualTo(NotificationIconType.Warning));
            Assert.That(_textHelper.GetIconType(new AdminNotification(NotificationTypes.CommentStatusChanged, null, null, null, null, null, null, null, DateTimeOffset.Now)), Is.EqualTo(NotificationIconType.Info));
            Assert.That(_textHelper.GetIconType(new AdminNotification(NotificationTypes.MovieCreated, null, null, null, null, null, null, null, DateTimeOffset.Now)), Is.EqualTo(NotificationIconType.Success));
            Assert.That(_textHelper.GetIconType(new AdminNotification(NotificationTypes.MovieDeleted, null, null, null, null, null, null, null, DateTimeOffset.Now)), Is.EqualTo(NotificationIconType.Warning));
        }
    }

    [Test]
    public void UnknownType_UsesFallbacks()
    {
        var n = new AdminNotification("UnknownType", null, null, null, null, null, null, null, DateTimeOffset.Now);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_textHelper.GetTitle(n), Is.EqualTo("Notification"));
            Assert.That(_textHelper.GetMessage(n), Is.EqualTo("New notification received"));
            Assert.That(_textHelper.GetIconType(n), Is.EqualTo(NotificationIconType.Info));
        }
    }

    [Test]
    public void Methods_ThrowOnNullArgument()
    {
        Assert.Throws<ArgumentNullException>(() => _textHelper.GetTitle(null!));
        Assert.Throws<ArgumentNullException>(() => _textHelper.GetMessage(null!));
        Assert.Throws<ArgumentNullException>(() => _textHelper.GetIconType(null!));
    }
}
