using Ardalis.Result;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Movies.Commands.DeleteMovie;
using FB_App.Domain.Entities;
using FB_App.Domain.Entities.Values;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace FB_App.Application.UnitTests.Movies.Commands.DeleteMovie;

[TestFixture]
public class DeleteMovieCommandHandlerTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<ICacheService> _cacheMock = null!;
    private Mock<DbSet<Movie>> _moviesDbSetMock = null!;
    private DeleteMovieCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _cacheMock = new Mock<ICacheService>();
        _moviesDbSetMock = new Mock<DbSet<Movie>>();
        _contextMock.Setup(x => x.Movies).Returns(_moviesDbSetMock.Object);
        _cacheMock.Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _handler = new DeleteMovieCommandHandler(_contextMock.Object, _cacheMock.Object);
    }

    [Test]
    public async Task Handle_WithExistingMovie_ShouldRemoveFromContext()
    {
        // Arrange

        var existingMovie = Movie.Create("Test Movie", null, null, null, null, null, null);
        var movieId = existingMovie.Id;
        _moviesDbSetMock.Setup(x => x.FindAsync(new object[] { movieId }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMovie);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new DeleteMovieCommand(movieId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _moviesDbSetMock.Verify(x => x.Remove(existingMovie), Times.Once);
    }

    [Test]
    public async Task Handle_WithExistingMovie_ShouldSaveChanges()
    {
        // Arrange
        var existingMovie = Movie.Create("Test Movie", null, null, null, null, null, null);
        var movieId = existingMovie.Id;
        _moviesDbSetMock.Setup(x => x.FindAsync(new object[] { movieId }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMovie);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new DeleteMovieCommand(movieId);

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
        _moviesDbSetMock.Setup(x => x.FindAsync(new object[] { movieId }, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Movie?)null);

        var command = new DeleteMovieCommand(movieId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Status, Is.EqualTo(ResultStatus.NotFound));
    }

    [Test]
    public async Task Handle_WithNonExistentMovie_ShouldIncludeMovieInErrors()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        _moviesDbSetMock.Setup(x => x.FindAsync(new object[] { movieId }, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Movie?)null);

        var command = new DeleteMovieCommand(movieId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Errors.Single().ShouldContain("Movie");
    }

    [Test]
    public async Task Handle_WithExistingMovie_ShouldAddDeletedDomainEvent()
    {
        // Arrange
        var existingMovie = Movie.Create("Test Movie", null, null, null, null, null, null);

        _moviesDbSetMock.Setup(x => x.FindAsync(new object[] { existingMovie.Id.Value }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMovie);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new DeleteMovieCommand(existingMovie.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingMovie.DomainEvents.ShouldNotBeEmpty();
    }

    [Test]
    public async Task Handle_WithCancellationToken_ShouldPassToFindAndSave()
    {
        // Arrange
        var existingMovie = Movie.Create("Test Movie", null, null, null, null, null, null);
        var movieId = existingMovie.Id;
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _moviesDbSetMock.Setup(x => x.FindAsync(new object[] { movieId }, token))
            .ReturnsAsync(existingMovie);
        _contextMock.Setup(x => x.SaveChangesAsync(token))
            .ReturnsAsync(1);

        var command = new DeleteMovieCommand(movieId);

        // Act
        await _handler.Handle(command, token);

        // Assert
        _contextMock.Verify(x => x.SaveChangesAsync(token), Times.Once);
    }
}
