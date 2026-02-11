using FB_App.Domain.Entities.Values;
using NUnit.Framework;

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
        Assert.That(commentId.Value, Is.EqualTo(value));
    }

    [Test]
    public void Create_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var value = Guid.Empty;

        // Act & Assert
        Assert.That(() => CommentId.Create(value), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void CreateNew_ShouldCreateCommentIdWithGeneratedGuid()
    {
        // Act
        var commentId = CommentId.CreateNew();

        // Assert
        Assert.That(commentId.Value, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public void TryCreate_WithValidGuid_ShouldSucceed()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var result = CommentId.TryCreate(value, out var commentId);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result, Is.True);
            Assert.That(commentId!.Value, Is.EqualTo(value));
        }
    }

    [Test]
    public void TryCreate_WithEmptyGuid_ShouldFail()
    {
        // Arrange
        var value = Guid.Empty;

        // Act
        var result = CommentId.TryCreate(value, out var commentId);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result, Is.False);
            Assert.That(commentId, Is.Null);
        }
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
        Assert.That(value, Is.EqualTo(guidValue));
    }

    [Test]
    public void ExplicitConversion_FromGuidToCommentId_ShouldCreateCommentId()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var commentId = (CommentId)value;

        // Assert
        Assert.That(commentId.Value, Is.EqualTo(value));
    }

    [Test]
    public void ExplicitConversion_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var value = Guid.Empty;

        // Act & Assert
        Assert.That(() => (CommentId)value, Throws.TypeOf<ArgumentException>());
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
        Assert.That(commentId1, Is.Not.EqualTo(commentId2));
        Assert.That(commentId1, Is.Not.EqualTo(commentId2));
    }

    [Test]
    public void GetHashCode_WithSameValue_ShouldHaveSameHashCode()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var commentId1 = CommentId.Create(guid);
        var commentId2 = CommentId.Create(guid);

        // Act & Assert
        Assert.That(commentId1.GetHashCode(), Is.EqualTo(commentId2.GetHashCode()));
    }

    [Test]
    public void GetHashCode_WithDifferentValues_ShouldHaveDifferentHashCode()
    {
        // Arrange
        var commentId1 = CommentId.Create(Guid.NewGuid());
        var commentId2 = CommentId.Create(Guid.NewGuid());

        // Act & Assert
        Assert.That(commentId1.GetHashCode(), Is.Not.EqualTo(commentId2.GetHashCode()));
    }
}
