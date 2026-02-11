using FB_App.Domain.Entities.Values;
using NUnit.Framework;
using Shouldly;

namespace FB_App.Domain.UnitTests.ValueObjects;

[TestFixture]
public class CommentIdTests
{
    [Test]
    public void Create_WithValidGuid_ShouldCreateCommentId()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var commentId = CommentId.Create(value);

        // Assert
        commentId.Value.ShouldBe(value);
    }

    [Test]
    public void Create_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var value = Guid.Empty;

        // Act & Assert
        Should.Throw<ArgumentException>(() => CommentId.Create(value));
    }

    [Test]
    public void CreateNew_ShouldCreateCommentIdWithGeneratedGuid()
    {
        // Act
        var commentId = CommentId.CreateNew();

        // Assert
        commentId.Value.ShouldNotBe(Guid.Empty);
    }

    [Test]
    public void TryCreate_WithValidGuid_ShouldSucceed()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var result = CommentId.TryCreate(value, out var commentId);

        // Assert
        result.ShouldBeTrue();
        commentId!.Value.ShouldBe(value);
    }

    [Test]
    public void TryCreate_WithEmptyGuid_ShouldFail()
    {
        // Arrange
        var value = Guid.Empty;

        // Act
        var result = CommentId.TryCreate(value, out var commentId);

        // Assert
        result.ShouldBeFalse();
        commentId.ShouldBeNull();
    }

    [Test]
    public void ImplicitConversion_FromCommentIdToGuid_ShouldReturnValue()
    {
        // Arrange
        var guidValue = Guid.NewGuid();
        var commentId = CommentId.Create(guidValue);

        // Act
        Guid value = commentId;

        // Assert
        value.ShouldBe(guidValue);
    }

    [Test]
    public void ExplicitConversion_FromGuidToCommentId_ShouldCreateCommentId()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var commentId = (CommentId)value;

        // Assert
        commentId.Value.ShouldBe(value);
    }

    [Test]
    public void ExplicitConversion_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var value = Guid.Empty;

        // Act & Assert
        Should.Throw<ArgumentException>(() => (CommentId)value);
    }

    [Test]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var commentId1 = CommentId.Create(guid);
        var commentId2 = CommentId.Create(guid);

        // Act & Assert
        Assert.That(commentId1, Is.EqualTo(commentId2));
    }

    [Test]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var commentId1 = CommentId.Create(Guid.NewGuid());
        var commentId2 = CommentId.Create(Guid.NewGuid());

        // Act & Assert
        commentId1.ShouldNotBe(commentId2);
        (commentId1 != commentId2).ShouldBeTrue();
    }

    [Test]
    public void GetHashCode_WithSameValue_ShouldHaveSameHashCode()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var commentId1 = CommentId.Create(guid);
        var commentId2 = CommentId.Create(guid);

        // Act & Assert
        commentId1.GetHashCode().ShouldBe(commentId2.GetHashCode());
    }

    [Test]
    public void GetHashCode_WithDifferentValues_ShouldHaveDifferentHashCode()
    {
        // Arrange
        var commentId1 = CommentId.Create(Guid.NewGuid());
        var commentId2 = CommentId.Create(Guid.NewGuid());

        // Act & Assert
        commentId1.GetHashCode().ShouldNotBe(commentId2.GetHashCode());
    }
}
