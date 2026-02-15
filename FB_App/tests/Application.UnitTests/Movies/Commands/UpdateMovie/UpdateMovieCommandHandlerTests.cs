using Ardalis.Result;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Movies.Commands.UpdateMovie;
using FB_App.Domain.Entities;
using FB_App.Domain.Entities.Values;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace FB_App.Application.UnitTests.Movies.Commands.UpdateMovie;

[TestFixture]
public class UpdateMovieCommandHandlerTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<ICacheService> _cacheMock = null!;
    private Mock<DbSet<Movie>> _moviesDbSetMock = null!;
    private UpdateMovieCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _cacheMock = new Mock<ICacheService>();
        _moviesDbSetMock = new Mock<DbSet<Movie>>();
        _contextMock.Setup(x => x.Movies).Returns(_moviesDbSetMock.Object);
        _cacheMock.Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _handler = new UpdateMovieCommandHandler(_contextMock.Object, _cacheMock.Object);
    }

    [Test]
    public async Task Handle_WithExistingMovie_ShouldUpdateAllFields()
    {
        // Arrange
        var existingMovie = Movie.Create("Original Title", "Original Description", 2020, "Original Director", "Drama", null, 7.0);

        _moviesDbSetMock.Setup(x => x.FindAsync(new object[] { existingMovie.Id}, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMovie);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new UpdateMovieCommand
        {
            Id = existingMovie.Id,
            Title = "Updated Title",
            Description = "Updated Description",
            ReleaseYear = 2024,
            Director = "Updated Director",
            Genre = "Action",
            PosterUrl = "https://example.com/new.jpg",
            Rating = 9.0
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(existingMovie.Title, Is.EqualTo("Updated Title"));
        Assert.That(existingMovie.Description, Is.EqualTo("Updated Description"));
        Assert.That(existingMovie.ReleaseYear, Is.EqualTo(2024));
        Assert.That(existingMovie.Director, Is.EqualTo("Updated Director"));
        Assert.That(existingMovie.Genre, Is.EqualTo("Action"));
        Assert.That(existingMovie.PosterUrl, Is.EqualTo("https://example.com/new.jpg"));
        Assert.That(existingMovie.Rating, Is.EqualTo(9.0));
    }

    [Test]
    public async Task Handle_WithExistingMovie_ShouldSaveChanges()
    {
        // Arrange
        var existingMovie = Movie.Create("Original Title", null, null, null, null, null, null);

        _moviesDbSetMock.Setup(x => x.FindAsync(new object[] { existingMovie.Id }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMovie);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new UpdateMovieCommand
        {
            Id = existingMovie.Id,
            Title = "Updated Title"
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_WithNonExistentMovie_ShouldReturnNotFoundResult()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        _moviesDbSetMock.Setup(x => x.FindAsync(new object[] { (MovieId)movieId }, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Movie?)null);

        var command = new UpdateMovieCommand
        {
            Id = movieId,
            Title = "Updated Title"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Status, Is.EqualTo(ResultStatus.NotFound));
    }

    [Test]
    public async Task Handle_WithNonExistentMovie_ShouldIncludeEntityNameInErrors()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        _moviesDbSetMock.Setup(x => x.FindAsync(new object[] { movieId }, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Movie?)null);

        var command = new UpdateMovieCommand
        {
            Id = movieId,
            Title = "Updated Title"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Errors.Single(), Does.Contain("Movie"));
    }

    [Test]
    public async Task Handle_WithCancellationToken_ShouldPassToFindAndSave()
    {
        // Arrange
        var existingMovie = Movie.Create("Original Title", null, null, null, null, null, null);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _moviesDbSetMock.Setup(x => x.FindAsync(new object[] { existingMovie.Id }, token))
            .ReturnsAsync(existingMovie);
        _contextMock.Setup(x => x.SaveChangesAsync(token))
            .ReturnsAsync(1);

        var command = new UpdateMovieCommand
        {
            Id = existingMovie.Id,
            Title = "Updated Title"
        };

        // Act
        await _handler.Handle(command, token);

        // Assert
        _contextMock.Verify(x => x.SaveChangesAsync(token), Times.Once);
    }

    [Test]
    public async Task Handle_WithNullOptionalFields_ShouldUpdateToNull()
    {
        // Arrange
        var existingMovie = Movie.Create("Title", "Has Description", 2020, "Director", "Genre", "url", 8.0);

        _moviesDbSetMock.Setup(x => x.FindAsync(new object[] { existingMovie.Id }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMovie);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new UpdateMovieCommand
        {
            Id = existingMovie.Id,
            Title = "Updated Title",
            Description = null,
            ReleaseYear = null,
            Director = null,
            Genre = null,
            PosterUrl = null,
            Rating = null
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(existingMovie.Description, Is.Null);
        Assert.That(existingMovie.ReleaseYear, Is.Null);
        Assert.That(existingMovie.Director, Is.Null);
        Assert.That(existingMovie.Genre, Is.Null);
        Assert.That(existingMovie.PosterUrl, Is.Null);
        Assert.That(existingMovie.Rating, Is.Null);
    }
}
