using FB_App.Domain.Entities.Values;
using NUnit.Framework;

namespace FB_App.Domain.UnitTests.ValueObjects;

[TestFixture]
public class MovieIdTests
{
    [Test]
    public void Create_WithValidGuid_ShouldCreateMovieId()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var movieId = MovieId.Create(value);

        // Assert
        Assert.That(movieId.Value, Is.EqualTo(value));
    }

    [Test]
    public void Create_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var value = Guid.Empty;

        // Act & Assert
        Assert.That(() => MovieId.Create(value), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void CreateNew_ShouldCreateMovieIdWithGeneratedGuid()
    {
        // Act
        var movieId = MovieId.CreateNew();

        // Assert
        Assert.That(movieId.Value, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public void TryCreate_WithValidGuid_ShouldSucceed()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var result = MovieId.TryCreate(value, out var movieId);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result, Is.True);
            Assert.That(movieId!.Value, Is.EqualTo(value));
        }
    }

    [Test]
    public void TryCreate_WithEmptyGuid_ShouldFail()
    {
        // Arrange
        var value = Guid.Empty;

        // Act
        var result = MovieId.TryCreate(value, out var movieId);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result, Is.False);
            Assert.That(movieId, Is.Null);
        }
    }

    [Test]
    public void ImplicitConversion_FromMovieIdToGuid_ShouldReturnValue()
    {
        // Arrange
        var guidValue = Guid.NewGuid();
        var movieId = MovieId.Create(guidValue);

        // Act
        Guid value = movieId;

        // Assert
        Assert.That(value, Is.EqualTo(guidValue));
    }

    [Test]
    public void ExplicitConversion_FromGuidToMovieId_ShouldCreateMovieId()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var movieId = (MovieId)value;

        // Assert
        Assert.That(movieId.Value, Is.EqualTo(value));
    }

    [Test]
    public void ExplicitConversion_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var value = Guid.Empty;

        // Act & Assert
        Assert.That(() => (MovieId)value, Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var movieId1 = MovieId.Create(guid);
        var movieId2 = MovieId.Create(guid);

        // Act & Assert
        Assert.That(movieId1, Is.EqualTo(movieId2));
    }

    [Test]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var movieId1 = MovieId.Create(Guid.NewGuid());
        var movieId2 = MovieId.Create(Guid.NewGuid());

        // Act & Assert
        Assert.That(movieId1, Is.Not.EqualTo(movieId2));
    }

    [Test]
    public void GetHashCode_WithSameValue_ShouldHaveSameHashCode()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var movieId1 = MovieId.Create(guid);
        var movieId2 = MovieId.Create(guid);

        // Act & Assert
        Assert.That(movieId1.GetHashCode(), Is.EqualTo(movieId2.GetHashCode()));
    }

    [Test]
    public void GetHashCode_WithDifferentValues_ShouldHaveDifferentHashCode()
    {
        // Arrange
        var movieId1 = MovieId.Create(Guid.NewGuid());
        var movieId2 = MovieId.Create(Guid.NewGuid());

        // Act & Assert
        Assert.That(movieId1.GetHashCode(), Is.Not.EqualTo(movieId2.GetHashCode()));
    }
}
