using FB_App.Domain.Entities;
using FB_App.Domain.Entities.Values;
using FB_App.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace FB_App.Domain.UnitTests.Entities;

[TestFixture]
public class MovieTests
{
    private Movie _movie = null!;
    private const string UserId = "user-123";

    [SetUp]
    public void SetUp()
    {
        _movie = new Movie
        {
            Title = "Test Movie",
            Description = "A test movie",
            ReleaseYear = 2024,
            Director = "Test Director",
            Genre = "Drama",
            PosterUrl = "https://example.com/poster.jpg",
            Rating = 8.5
        };
    }

    [Test]
    public void Create_NewMovie_ShouldHaveEmptyComments()
    {
        // Assert
        _movie.Comments.ShouldBeEmpty();
        _movie.TotalCommentsCount.ShouldBe(0);
        _movie.ApprovedCommentsCount.ShouldBe(0);
        _movie.PendingCommentsCount.ShouldBe(0);
        _movie.RejectedCommentsCount.ShouldBe(0);
    }

    [Test]
    public void AddComment_WithValidData_ShouldAddComment()
    {
        // Arrange
        const string commentText = "Great movie!";

        // Act
        var comment = _movie.AddComment(UserId, commentText);

        // Assert
        comment.ShouldNotBeNull();
        comment.UserId.ShouldBe(UserId);
        comment.Text.ShouldBe(commentText);
        comment.Status.ShouldBe(CommentStatus.Pending);
        comment.IsPending.ShouldBeTrue();
        _movie.Comments.ShouldContain(comment);
        _movie.TotalCommentsCount.ShouldBe(1);
        _movie.PendingCommentsCount.ShouldBe(1);
    }

    [Test]
    public void AddComment_WithNullUserId_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => _movie.AddComment(null!, "Great movie!"));
    }

    [Test]
    public void AddComment_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => _movie.AddComment("", "Great movie!"));
    }

    [Test]
    public void AddComment_WithNullText_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => _movie.AddComment(UserId, null!));
    }

    [Test]
    public void AddComment_WithEmptyText_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => _movie.AddComment(UserId, ""));
    }

    [Test]
    public void AddComment_MultipleComments_ShouldMaintainCount()
    {
        // Act
        _movie.AddComment(UserId, "First comment");
        _movie.AddComment("user-456", "Second comment");
        _movie.AddComment("user-789", "Third comment");

        // Assert
        _movie.TotalCommentsCount.ShouldBe(3);
        _movie.PendingCommentsCount.ShouldBe(3);
        _movie.Comments.Count.ShouldBe(3);
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
        approvedComment.ShouldNotBeNull();
        approvedComment.Status.ShouldBe(CommentStatus.Approved);
        approvedComment.IsApproved.ShouldBeTrue();
        approvedComment.ReviewedBy.ShouldBe(reviewerId);
        approvedComment.ReviewedAt.ShouldNotBeNull();
        _movie.ApprovedCommentsCount.ShouldBe(1);
        _movie.PendingCommentsCount.ShouldBe(0);
    }

   

    [Test]
    public void ApproveComment_WithAlreadyApprovedComment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var comment = _movie.AddComment(UserId, "Great movie!");
        var commentId = comment.Id;
        _movie.ApproveComment(commentId, "reviewer-123");

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => _movie.ApproveComment(commentId, "another-reviewer"));
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
        rejectedComment.ShouldNotBeNull();
        rejectedComment.Status.ShouldBe(CommentStatus.Rejected);
        rejectedComment.ReviewedBy.ShouldBe(reviewerId);
        rejectedComment.ReviewedAt.ShouldNotBeNull();
        _movie.RejectedCommentsCount.ShouldBe(1);
        _movie.PendingCommentsCount.ShouldBe(0);
    }

    

    [Test]
    public void RejectComment_WithAlreadyApprovedComment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var comment = _movie.AddComment(UserId, "Great movie!");
        var commentId = comment.Id;
        _movie.ApproveComment(commentId, "reviewer-123");

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => _movie.RejectComment(commentId, "another-reviewer"));
    }

    [Test]
    public void RemoveComment_WithExistingComment_ShouldRemoveSuccessfully()
    {
        // Arrange
        var comment = _movie.AddComment(UserId, "Great movie!");
        var commentId = comment.Id;

        // Act
        var result = _movie.RemoveComment(commentId);

        // Assert
        result.ShouldBeTrue();
        _movie.Comments.ShouldNotContain(comment);
        _movie.TotalCommentsCount.ShouldBe(0);
        _movie.GetComment(commentId).ShouldBeNull();
    }

    [Test]
    public void RemoveComment_WithNonExistentCommentId_ShouldReturnFalse()
    {
        // Act
        var result = _movie.RemoveComment(CommentId.CreateNew());

        // Assert
        result.ShouldBeFalse();
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
        retrievedComment.ShouldNotBeNull();
        retrievedComment!.UserId.ShouldBe(UserId);
        retrievedComment.Text.ShouldBe("Great movie!");
    }

    [Test]
    public void GetComment_WithNonExistentCommentId_ShouldReturnNull()
    {
        // Act
        var result = _movie.GetComment(CommentId.CreateNew());

        // Assert
        result.ShouldBeNull();
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
        approvedComments.Count.ShouldBe(2);
        approvedComments.ShouldContain(comment1);
        approvedComments.ShouldContain(comment2);
        approvedComments.ShouldNotContain(comment3);
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
        pendingComments.Count.ShouldBe(2);
        pendingComments.ShouldContain(comment1);
        pendingComments.ShouldContain(comment2);
        pendingComments.ShouldNotContain(comment3);
    }

    [Test]
    public void HasPendingComments_WhenHasPendingComments_ShouldReturnTrue()
    {
        // Arrange
        _movie.AddComment(UserId, "Pending comment");

        // Act
        var result = _movie.HasPendingComments;

        // Assert
        result.ShouldBeTrue();
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
        result.ShouldBeFalse();
    }

    [Test]
    public void MovieId_Property_ShouldReturnStronglyTypedId()
    {
        // Act
        var movieId = _movie.Id;

        // Assert
        movieId.Value.ShouldBe(_movie.Id);
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

        // Assert
        _movie.TotalCommentsCount.ShouldBe(2);
        _movie.ApprovedCommentsCount.ShouldBe(1);
        _movie.RejectedCommentsCount.ShouldBe(1);
        _movie.PendingCommentsCount.ShouldBe(0);
        _movie.GetComment(comment1.Id).ShouldNotBeNull();
        _movie.GetComment(comment2.Id).ShouldNotBeNull();
        _movie.GetComment(comment3.Id).ShouldBeNull();
    }
}
