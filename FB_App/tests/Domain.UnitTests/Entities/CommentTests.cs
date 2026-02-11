using FB_App.Domain.Entities;
using FB_App.Domain.Entities.Values;
using FB_App.Domain.Enums;
using NUnit.Framework;
namespace FB_App.Domain.UnitTests.Entities;

[TestFixture]
public class CommentTests
{
    private MovieId _movieId = null!;
    private const string UserId = "user-123";
    private const string CommentText = "Great movie!";
    [SetUp]
    public void SetUp()
    {
        _movieId = MovieId.CreateNew();
    }

    [Test]
    public void Create_WithValidData_ShouldCreateCommentInPendingStatus()
    {
        // Act
        var comment = Comment.Create(_movieId, UserId, CommentText);

        // Assert
        Assert.That(comment, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(comment.MovieId, Is.EqualTo(_movieId));
            Assert.That(comment.UserId, Is.EqualTo(UserId));
            Assert.That(comment.Text, Is.EqualTo(CommentText));
            Assert.That(comment.Status, Is.EqualTo(CommentStatus.Pending));
            Assert.That(comment.IsPending, Is.True);
            Assert.That(comment.IsApproved, Is.False);
            Assert.That(comment.IsReviewed, Is.False);
            Assert.That(comment.ReviewedBy, Is.Null);
            Assert.That(comment.ReviewedAt, Is.Null);
        }
    }

    [Test]
    public void Create_WithNullUserId_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.That(() => Comment.Create(_movieId, null!, CommentText), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Create_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.That(() => Comment.Create(_movieId, "", CommentText), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Create_WithWhitespaceUserId_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.That(() => Comment.Create(_movieId, "   ", CommentText), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Create_WithNullText_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.That(() => Comment.Create(_movieId, UserId, null!), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Create_WithEmptyText_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.That(() => Comment.Create(_movieId, UserId, ""), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Create_WithWhitespaceText_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.That(() => Comment.Create(_movieId, UserId, "   "), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Approve_WithPendingComment_ShouldApproveSuccessfully()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);
        const string reviewedBy = "reviewer-123";

        // Act
        comment.Approve(reviewedBy);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(comment.Status, Is.EqualTo(CommentStatus.Approved));
            Assert.That(comment.IsApproved, Is.True);
            Assert.That(comment.IsPending, Is.False);
            Assert.That(comment.ReviewedBy, Is.EqualTo(reviewedBy));
            Assert.That(comment.ReviewedAt, Is.Not.Null);
            Assert.That(comment.IsReviewed, Is.True);
        }
    }

    [Test]
    public void Approve_WithNullReviewedBy_ShouldThrowArgumentException()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);

        // Act & Assert
        Assert.That(() => comment.Approve(null!), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Approve_WithEmptyReviewedBy_ShouldThrowArgumentException()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);

        // Act & Assert
        Assert.That(() => comment.Approve(""), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Approve_WithWhitespaceReviewedBy_ShouldThrowArgumentException()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);

        // Act & Assert
        Assert.That(() => comment.Approve("   "), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Approve_WithAlreadyApprovedComment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);
        comment.Approve("reviewer-123");

        // Act & Assert
        Assert.That(() => comment.Approve("another-reviewer"), Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void Approve_WithRejectedComment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);
        comment.Reject("reviewer-123");

        // Act & Assert
        Assert.That(() => comment.Approve("another-reviewer"), Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void Reject_WithPendingComment_ShouldRejectSuccessfully()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);
        const string reviewedBy = "reviewer-123";

        // Act
        comment.Reject(reviewedBy);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(comment.Status, Is.EqualTo(CommentStatus.Rejected));
            Assert.That(comment.IsApproved, Is.False);
            Assert.That(comment.IsPending, Is.False);
            Assert.That(comment.ReviewedBy, Is.EqualTo(reviewedBy));
            Assert.That(comment.ReviewedAt, Is.Not.Null);
            Assert.That(comment.IsReviewed, Is.True);
        }
    }

    [Test]
    public void Reject_WithNullReviewedBy_ShouldThrowArgumentException()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);

        // Act & Assert
        Assert.That(() => comment.Reject(null!), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Reject_WithEmptyReviewedBy_ShouldThrowArgumentException()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);

        // Act & Assert
        Assert.That(() => comment.Reject(""), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Reject_WithWhitespaceReviewedBy_ShouldThrowArgumentException()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);

        // Act & Assert
        Assert.That(() => comment.Reject("   "), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Reject_WithAlreadyRejectedComment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);
        comment.Reject("reviewer-123");

        // Act & Assert
        Assert.That(() => comment.Reject("another-reviewer"), Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void Reject_WithApprovedComment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);
        comment.Approve("reviewer-123");

        // Act & Assert
        Assert.That(() => comment.Reject("another-reviewer"), Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void ResetToPending_WithApprovedComment_ShouldResetSuccessfully()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);
        comment.Approve("reviewer-123");

        // Act
        comment.ResetToPending();

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(comment.Status, Is.EqualTo(CommentStatus.Pending));
            Assert.That(comment.IsPending, Is.True);
            Assert.That(comment.IsApproved, Is.False);
            Assert.That(comment.IsReviewed, Is.False);
            Assert.That(comment.ReviewedBy, Is.Null);
            Assert.That(comment.ReviewedAt, Is.Null);
        }
    }

    [Test]
    public void ResetToPending_WithRejectedComment_ShouldResetSuccessfully()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);
        comment.Reject("reviewer-123");

        // Act
        comment.ResetToPending();

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(comment.Status, Is.EqualTo(CommentStatus.Pending));
            Assert.That(comment.IsPending, Is.True);
            Assert.That(comment.IsApproved, Is.False);
            Assert.That(comment.IsReviewed, Is.False);
            Assert.That(comment.ReviewedBy, Is.Null);
            Assert.That(comment.ReviewedAt, Is.Null);
        }
    }

    [Test]
    public void ResetToPending_WithAlreadyPendingComment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);

        // Act & Assert
        Assert.That(() => comment.ResetToPending(), Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void IsPending_WithPendingComment_ShouldReturnTrue()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);

        // Act & Assert
        Assert.That(comment.IsPending, Is.True);
    }

    [Test]
    public void IsPending_WithApprovedComment_ShouldReturnFalse()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);
        comment.Approve("reviewer-123");

        // Act & Assert
        Assert.That(comment.IsPending, Is.False);
    }

    [Test]
    public void IsPending_WithRejectedComment_ShouldReturnFalse()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);
        comment.Reject("reviewer-123");

        // Act & Assert
        Assert.That(comment.IsPending, Is.False);
    }

    [Test]
    public void IsApproved_WithApprovedComment_ShouldReturnTrue()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);
        comment.Approve("reviewer-123");

        // Act & Assert
        Assert.That(comment.IsApproved, Is.True);
    }

    [Test]
    public void IsApproved_WithPendingComment_ShouldReturnFalse()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);

        // Act & Assert
        Assert.That(comment.IsApproved, Is.False);
    }

    [Test]
    public void IsApproved_WithRejectedComment_ShouldReturnFalse()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);
        comment.Reject("reviewer-123");

        // Act & Assert
        Assert.That(comment.IsApproved, Is.False);
    }

    [Test]
    public void IsReviewed_WithApprovedComment_ShouldReturnTrue()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);
        comment.Approve("reviewer-123");

        // Act & Assert
        Assert.That(comment.IsReviewed, Is.True);
    }

    [Test]
    public void IsReviewed_WithRejectedComment_ShouldReturnTrue()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);
        comment.Reject("reviewer-123");

        // Act & Assert
        Assert.That(comment.IsReviewed, Is.True);
    }

    [Test]
    public void IsReviewed_WithPendingComment_ShouldReturnFalse()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);

        // Act & Assert
        Assert.That(comment.IsReviewed, Is.False);
    }

    [Test]
    public void IsReviewed_WithResetComment_ShouldReturnFalse()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);
        comment.Approve("reviewer-123");
        comment.ResetToPending();

        // Act & Assert
        Assert.That(comment.IsReviewed, Is.False);
    }

    [Test]
    public void ReviewTimestamp_AfterApprove_ShouldBeSetToUtcNow()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);
        var beforeApproval = DateTimeOffset.UtcNow.AddSeconds(-1);

        // Act
        comment.Approve("reviewer-123");
        var afterApproval = DateTimeOffset.UtcNow.AddSeconds(1);

        // Assert
        Assert.That(comment.ReviewedAt, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(comment.ReviewedAt!.Value, Is.GreaterThanOrEqualTo(beforeApproval));
            Assert.That(comment.ReviewedAt.Value, Is.LessThanOrEqualTo(afterApproval));
        }
    }

    [Test]
    public void ReviewTimestamp_AfterReject_ShouldBeSetToUtcNow()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);
        var beforeReject = DateTimeOffset.UtcNow.AddSeconds(-1);

        // Act
        comment.Reject("reviewer-123");
        var afterReject = DateTimeOffset.UtcNow.AddSeconds(1);

        // Assert
        Assert.That(comment.ReviewedAt, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(comment.ReviewedAt!.Value, Is.GreaterThanOrEqualTo(beforeReject));
            Assert.That(comment.ReviewedAt.Value, Is.LessThanOrEqualTo(afterReject));
        }
    }

    [Test]
    public void StatusTransitionScenario_ApproveRejectReset()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, CommentText);

        // Act 1: Approve
        comment.Approve("reviewer-123");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(comment.IsApproved, Is.True);
            Assert.That(comment.IsPending, Is.False);
        }

        // Act 2: Reset to Pending
        comment.ResetToPending();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(comment.IsPending, Is.True);
            Assert.That(comment.IsApproved, Is.False);
        }

        // Act 3: Reject
        comment.Reject("reviewer-456");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(comment.Status, Is.EqualTo(CommentStatus.Rejected));
            Assert.That(comment.IsPending, Is.False);
        }

        // Act 4: Reset to Pending again
        comment.ResetToPending();
        Assert.That(comment.IsPending, Is.True);

        // Act 5: Approve again
        comment.Approve("reviewer-789");
        Assert.That(comment.IsApproved, Is.True);
    }

    [Test]
    public void ComplexScenario_MultipleStateChanges()
    {
        // Arrange
        var comment = Comment.Create(_movieId, UserId, "This is a complex test");

        using (Assert.EnterMultipleScope())
        {
            // Initial state
            Assert.That(comment.IsPending, Is.True);
            Assert.That(comment.IsReviewed, Is.False);
        }

        // Approve
        comment.Approve("admin-1");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(comment.IsApproved, Is.True);
            Assert.That(comment.ReviewedBy, Is.EqualTo("admin-1"));
        }

        // Reset
        comment.ResetToPending();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(comment.ReviewedBy, Is.Null);
            Assert.That(comment.ReviewedAt, Is.Null);
        }

        // Reject
        comment.Reject("admin-2");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(comment.Status, Is.EqualTo(CommentStatus.Rejected));
            Assert.That(comment.ReviewedBy, Is.EqualTo("admin-2"));
            Assert.That(comment.ReviewedAt, Is.Not.Null);
        }
    }
}
