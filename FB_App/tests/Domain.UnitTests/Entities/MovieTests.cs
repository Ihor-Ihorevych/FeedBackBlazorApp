using FB_App.Domain.Entities;
using FB_App.Domain.Entities.Values;
using FB_App.Domain.Enums;
using NUnit.Framework;

namespace FB_App.Domain.UnitTests.Entities;

[TestFixture]
public class MovieTests
{
    private Movie _movie = null!;
    private const string UserId = "user-123";

    [SetUp]
    public void SetUp()
    {
        _movie = Movie.Create("Test Movie",
                              "A test movie",
                              2024,
                              "Test Director",
                              "Drama",
                              "https://example.com/poster.jpg",
                              8.5);
    }

    [Test]
    public void Create_NewMovie_ShouldHaveEmptyComments()
    {
        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(_movie.Comments, Is.Empty);
            Assert.That(_movie.TotalCommentsCount, Is.Zero);
            Assert.That(_movie.ApprovedCommentsCount, Is.Zero);
            Assert.That(_movie.PendingCommentsCount, Is.Zero);
            Assert.That(_movie.RejectedCommentsCount, Is.Zero);
        }
    }

    [Test]
    public void AddComment_WithValidData_ShouldAddComment()
    {
        // Arrange
        const string commentText = "Great movie!";

        // Act
        var comment = _movie.AddComment(UserId, commentText);

        // Assert
        Assert.That(comment, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(comment.UserId, Is.EqualTo(UserId));
            Assert.That(comment.Text, Is.EqualTo(commentText));
            Assert.That(comment.Status, Is.EqualTo(CommentStatus.Pending));
            Assert.That(comment.IsPending, Is.True);
            Assert.That(_movie.Comments, Does.Contain(comment));
            Assert.That(_movie.TotalCommentsCount, Is.EqualTo(1));
            Assert.That(_movie.PendingCommentsCount, Is.EqualTo(1));
        }
    }

    [Test]
    public void AddComment_WithNullUserId_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.That(() => _movie.AddComment(null!, "Great movie!"),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void AddComment_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.That(() => _movie.AddComment("", "Great movie!"),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void AddComment_WithNullText_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.That(() => _movie.AddComment(UserId, null!),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void AddComment_WithEmptyText_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.That(() => _movie.AddComment(UserId, ""),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void AddComment_MultipleComments_ShouldMaintainCount()
    {
        // Act
        _movie.AddComment(UserId, "First comment");
        _movie.AddComment("user-456", "Second comment");
        _movie.AddComment("user-789", "Third comment");

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(_movie.TotalCommentsCount, Is.EqualTo(3));
            Assert.That(_movie.PendingCommentsCount, Is.EqualTo(3));
            Assert.That(_movie.Comments, Has.Count.EqualTo(3));
        }
    }

    [Test]
    public void ApproveComment_WithPendingComment_ShouldApproveSuccessfully()
    {
        // Arrange
        var comment = _movie.AddComment(UserId, "Great movie!");
        var commentId = comment.Id;
        const string reviewerId = "reviewer-123";

        // Act
        _movie.ApproveComment(commentId, reviewerId);

        // Assert
        var approvedComment = _movie.GetComment(commentId);
        Assert.That(approvedComment, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(approvedComment!.Status, Is.EqualTo(CommentStatus.Approved));
            Assert.That(approvedComment.IsApproved, Is.True);
            Assert.That(approvedComment.ReviewedBy, Is.EqualTo(reviewerId));
            Assert.That(approvedComment.ReviewedAt, Is.Not.Null);
            Assert.That(_movie.ApprovedCommentsCount, Is.EqualTo(1));
            Assert.That(_movie.PendingCommentsCount, Is.Zero);
        }
    }

    [Test]
    public void ApproveComment_WithAlreadyApprovedComment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var comment = _movie.AddComment(UserId, "Great movie!");
        var commentId = comment.Id;
        _movie.ApproveComment(commentId, "reviewer-123");

        // Act & Assert
        Assert.That(() => _movie.ApproveComment(commentId, "another-reviewer"),
            Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void RejectComment_WithPendingComment_ShouldRejectSuccessfully()
    {
        // Arrange
        var comment = _movie.AddComment(UserId, "Inappropriate comment");
        var commentId = comment.Id;
        const string reviewerId = "reviewer-123";

        // Act
        _movie.RejectComment(commentId, reviewerId);

        // Assert
        var rejectedComment = _movie.GetComment(commentId);
        Assert.That(rejectedComment, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(rejectedComment!.Status, Is.EqualTo(CommentStatus.Rejected));
            Assert.That(rejectedComment.ReviewedBy, Is.EqualTo(reviewerId));
            Assert.That(rejectedComment.ReviewedAt, Is.Not.Null);
            Assert.That(_movie.RejectedCommentsCount, Is.EqualTo(1));
            Assert.That(_movie.PendingCommentsCount, Is.Zero);
        }
    }

    [Test]
    public void RejectComment_WithAlreadyApprovedComment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var comment = _movie.AddComment(UserId, "Great movie!");
        var commentId = comment.Id;
        _movie.ApproveComment(commentId, "reviewer-123");

        // Act & Assert
        Assert.That(() => _movie.RejectComment(commentId, "another-reviewer"),
            Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void RemoveComment_WithExistingComment_ShouldRemoveSuccessfully()
    {
        // Arrange
        var comment = _movie.AddComment(UserId, "Great movie!");
        var commentId = comment.Id;

        // Act
        var result = _movie.RemoveComment(commentId);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result, Is.True);
            Assert.That(_movie.Comments, Does.Not.Contain(comment));
            Assert.That(_movie.TotalCommentsCount, Is.Zero);
            Assert.That(_movie.GetComment(commentId), Is.Null);
        }
    }

    [Test]
    public void RemoveComment_WithNonExistentCommentId_ShouldReturnFalse()
    {
        // Act
        var result = _movie.RemoveComment(CommentId.CreateNew());

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void GetComment_WithExistingComment_ShouldReturnComment()
    {
        // Arrange
        var comment = _movie.AddComment(UserId, "Great movie!");
        var commentId = comment.Id;

        // Act
        var retrievedComment = _movie.GetComment(commentId);

        // Assert
        Assert.That(retrievedComment, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(retrievedComment!.UserId, Is.EqualTo(UserId));
            Assert.That(retrievedComment.Text, Is.EqualTo("Great movie!"));
        }
    }

    [Test]
    public void GetComment_WithNonExistentCommentId_ShouldReturnNull()
    {
        // Act
        var result = _movie.GetComment(CommentId.CreateNew());

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetApprovedComments_ShouldReturnOnlyApprovedComments()
    {
        // Arrange
        var comment1 = _movie.AddComment(UserId, "Great movie!");
        var comment2 = _movie.AddComment("user-456", "Excellent!");
        var comment3 = _movie.AddComment("user-789", "Not good");

        _movie.ApproveComment(comment1.Id, "reviewer-123");
        _movie.ApproveComment(comment2.Id, "reviewer-123");
        _movie.RejectComment(comment3.Id, "reviewer-123");

        // Act
        var approvedComments = _movie.GetApprovedComments();

        // Assert
        Assert.That(approvedComments, Has.Count.EqualTo(2));
        Assert.That(approvedComments, Does.Contain(comment1));
        Assert.That(approvedComments, Does.Contain(comment2));
        Assert.That(approvedComments, Does.Not.Contain(comment3));
    }

    [Test]
    public void GetPendingComments_ShouldReturnOnlyPendingComments()
    {
        // Arrange
        var comment1 = _movie.AddComment(UserId, "Pending comment 1");
        var comment2 = _movie.AddComment("user-456", "Pending comment 2");
        var comment3 = _movie.AddComment("user-789", "To be approved");

        _movie.ApproveComment(comment3.Id, "reviewer-123");

        // Act
        var pendingComments = _movie.GetPendingComments();

        // Assert
        Assert.That(pendingComments, Has.Count.EqualTo(2));
        Assert.That(pendingComments, Does.Contain(comment1));
        Assert.That(pendingComments, Does.Contain(comment2));
        Assert.That(pendingComments, Does.Not.Contain(comment3));
    }

    [Test]
    public void HasPendingComments_WhenHasPendingComments_ShouldReturnTrue()
    {
        // Arrange
        _movie.AddComment(UserId, "Pending comment");

        // Act
        var result = _movie.HasPendingComments;

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void HasPendingComments_WhenNoPendingComments_ShouldReturnFalse()
    {
        // Arrange
        var comment = _movie.AddComment(UserId, "Comment");
        _movie.ApproveComment(comment.Id, "reviewer-123");

        // Act
        var result = _movie.HasPendingComments;

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void MovieId_Property_ShouldReturnStronglyTypedId()
    {
        // Act
        var movieId = _movie.Id;

        // Assert
        Assert.That(movieId, Is.EqualTo((MovieId)_movie.Id.Value));
    }

    [Test]
    public void ComplexScenario_ApproveRejectAndRemove()
    {
        // Arrange & Act
        var comment1 = _movie.AddComment(UserId, "Good movie");
        var comment2 = _movie.AddComment("user-456", "Bad movie");
        var comment3 = _movie.AddComment("user-789", "To be removed");

        _movie.ApproveComment(comment1.Id, "reviewer-123");
        _movie.RejectComment(comment2.Id, "reviewer-123");
        _movie.RemoveComment(comment3.Id);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(_movie.TotalCommentsCount, Is.EqualTo(2));
            Assert.That(_movie.ApprovedCommentsCount, Is.EqualTo(1));
            Assert.That(_movie.RejectedCommentsCount, Is.EqualTo(1));
            Assert.That(_movie.PendingCommentsCount, Is.Zero);
            Assert.That(_movie.GetComment(comment1.Id), Is.Not.Null);
            Assert.That(_movie.GetComment(comment2.Id), Is.Not.Null);
            Assert.That(_movie.GetComment(comment3.Id), Is.Null);
        }
    }
}
