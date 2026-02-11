using FB_App.Domain.Entities.Values;
using NUnit.Framework;
using Shouldly;

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
        movieId.Value.ShouldBe(value);
    }

    [Test]
    public void Create_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var value = Guid.Empty;

        // Act & Assert
        Should.Throw<ArgumentException>(() => MovieId.Create(value));
    }

    [Test]
    public void CreateNew_ShouldCreateMovieIdWithGeneratedGuid()
    {
        // Act
        var movieId = MovieId.CreateNew();

        // Assert
        movieId.Value.ShouldNotBe(Guid.Empty);
    }

    [Test]
    public void TryCreate_WithValidGuid_ShouldSucceed()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var result = MovieId.TryCreate(value, out var movieId);

        // Assert
        result.ShouldBeTrue();
        movieId!.Value.ShouldBe(value);
    }

    [Test]
    public void TryCreate_WithEmptyGuid_ShouldFail()
    {
        // Arrange
        var value = Guid.Empty;

        // Act
        var result = MovieId.TryCreate(value, out var movieId);

        // Assert
        result.ShouldBeFalse();
        movieId.ShouldBeNull();
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
        value.ShouldBe(guidValue);
    }

    [Test]
    public void ExplicitConversion_FromGuidToMovieId_ShouldCreateMovieId()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var movieId = (MovieId)value;

        // Assert
        movieId.Value.ShouldBe(value);
    }

    [Test]
    public void ExplicitConversion_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var value = Guid.Empty;

        // Act & Assert
        Should.Throw<ArgumentException>(() => (MovieId)value);
    }

    [Test]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var movieId1 = MovieId.Create(guid);
        var movieId2 = MovieId.Create(guid);

        Assert.That(movieId1, Is.EqualTo(movieId2));
    }

    [Test]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var movieId1 = MovieId.Create(Guid.NewGuid());
        var movieId2 = MovieId.Create(Guid.NewGuid());

        // Act & Assert
        movieId1.ShouldNotBe(movieId2);
        (movieId1 != movieId2).ShouldBeTrue();
    }

    [Test]
    public void GetHashCode_WithSameValue_ShouldHaveSameHashCode()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var movieId1 = MovieId.Create(guid);
        var movieId2 = MovieId.Create(guid);

        // Act & Assert
        movieId1.GetHashCode().ShouldBe(movieId2.GetHashCode());
    }

    [Test]
    public void GetHashCode_WithDifferentValues_ShouldHaveDifferentHashCode()
    {
        // Arrange
        var movieId1 = MovieId.Create(Guid.NewGuid());
        var movieId2 = MovieId.Create(Guid.NewGuid());

        // Act & Assert
        movieId1.GetHashCode().ShouldNotBe(movieId2.GetHashCode());
    }
}
