using FB_App.Application.Common.Interfaces;
using FB_App.Application.Movies.Commands.CreateMovie;
using FB_App.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FB_App.Application.UnitTests.Movies.Commands.CreateMovie;

[TestFixture]
public class CreateMovieCommandHandlerTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<DbSet<Movie>> _moviesDbSetMock = null!;
    private CreateMovieCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _moviesDbSetMock = new Mock<DbSet<Movie>>();
        _contextMock.Setup(x => x.Movies).Returns(_moviesDbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _handler = new CreateMovieCommandHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_WithValidCommand_ShouldAddMovieToContext()
    {
        // Arrange
        var command = new CreateMovieCommand
        {
            Title = "Test Movie",
            Description = "A test movie description",
            ReleaseYear = 2024,
            Director = "Test Director",
            Genre = "Action",
            PosterUrl = "https://example.com/poster.jpg",
            Rating = 8.5
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _moviesDbSetMock.Verify(x => x.Add(It.Is<Movie>(m =>
            m.Title == command.Title &&
            m.Description == command.Description &&
            m.ReleaseYear == command.ReleaseYear &&
            m.Director == command.Director &&
            m.Genre == command.Genre)), Times.Once);
    }

    [Test]
    public async Task Handle_WithValidCommand_ShouldSaveChanges()
    {
        // Arrange
        var command = new CreateMovieCommand
        {
            Title = "Test Movie",
            Description = "A test movie description"
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_WithValidCommand_ShouldReturnMovieId()
    {
        // Arrange
        var command = new CreateMovieCommand
        {
            Title = "Test Movie"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task Handle_ShouldCreateMovieWithCorrectTitle()
    {
        // Arrange
        var command = new CreateMovieCommand
        {
            Title = "Inception"
        };
        Movie? addedMovie = null;
        _moviesDbSetMock.Setup(x => x.Add(It.IsAny<Movie>()))
            .Callback<Movie>(movie => addedMovie = movie);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(addedMovie, Is.Not.Null);
        Assert.That(addedMovie!.Title, Is.EqualTo("Inception"));
    }

    [Test]
    public async Task Handle_ShouldCreateMovieWithOptionalFieldsAsNull()
    {
        // Arrange
        var command = new CreateMovieCommand
        {
            Title = "Minimal Movie"
        };
        Movie? addedMovie = null;
        _moviesDbSetMock.Setup(x => x.Add(It.IsAny<Movie>()))
            .Callback<Movie>(movie => addedMovie = movie);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(addedMovie, Is.Not.Null);
        Assert.That(addedMovie!.Title, Is.EqualTo("Minimal Movie"));
        Assert.That(addedMovie.Description, Is.Null);
        Assert.That(addedMovie.Director, Is.Null);
        Assert.That(addedMovie.Genre, Is.Null);
    }

    [Test]
    public async Task Handle_WithCancellationToken_ShouldPassToSaveChanges()
    {
        // Arrange
        var command = new CreateMovieCommand { Title = "Test Movie" };
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        await _handler.Handle(command, token);

        // Assert
        _contextMock.Verify(x => x.SaveChangesAsync(token), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldAddDomainEvent()
    {
        // Arrange
        var command = new CreateMovieCommand { Title = "Test Movie" };
        Movie? addedMovie = null;
        _moviesDbSetMock.Setup(x => x.Add(It.IsAny<Movie>()))
            .Callback<Movie>(movie => addedMovie = movie);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(addedMovie, Is.Not.Null);
        Assert.That(addedMovie!.DomainEvents, Is.Not.Empty);
    }
}
