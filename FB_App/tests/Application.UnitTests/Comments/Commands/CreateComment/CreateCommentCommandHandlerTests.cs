using FB_App.Application.Comments.Commands.CreateComment;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.UnitTests.Common.Testing;
using FB_App.Domain.Entities;
using FB_App.Domain.Entities.Values;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Ardalis.Result;

namespace FB_App.Application.UnitTests.Comments.Commands.CreateComment;

[TestFixture]
public class CreateCommentCommandHandlerTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IUser> _userMock = null!;
    private CreateCommentCommandHandler _handler = null!;
    private List<Movie> _movies = null!;

    [SetUp]
    public void SetUp()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _userMock = new Mock<IUser>();
        _movies = [];

        var moviesDbSetMock = CreateMockDbSet(_movies);
        _contextMock.Setup(x => x.Movies).Returns(moviesDbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _userMock.Setup(x => x.Id).Returns("user-123");

        _handler = new CreateCommentCommandHandler(_contextMock.Object, _userMock.Object);
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        return mockSet;
    }

    [Test]
    public async Task Handle_WithValidMovieAndUser_ShouldAddCommentAndReturnId()
    {
        // Arrange
        var movie = Movie.Create("Test Movie", null, null, null, null, null, null);
        _movies.Add(movie);

        var command = new CreateCommentCommand
        {
            MovieId = movie.Id,
            Text = "Great movie!"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result, Is.Not.Default);
            Assert.That(movie.Comments, Is.Not.Empty);
        }
    }

    [Test]
    public async Task Handle_WithValidCommand_ShouldSaveChanges()
    {
        // Arrange
        var movie = Movie.Create("Test Movie", null, null, null, null, null, null);
        _movies.Add(movie);

        var command = new CreateCommentCommand
        {
            MovieId = movie.Id,
            Text = "Great movie!"
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
        var command = new CreateCommentCommand
        {
            MovieId = Guid.NewGuid(),
            Text = "Great movie!"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That(result.Errors.Single(), Does.Contain("Movie"));
    }

    [Test]
    public async Task Handle_WithNullUserId_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        var movie = Movie.Create("Test Movie", null, null, null, null, null, null);
        _movies.Add(movie);
        _userMock.Setup(x => x.Id).Returns((string?)null);

        var command = new CreateCommentCommand
        {
            MovieId = movie.Id,
            Text = "Great movie!"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Status, Is.EqualTo(ResultStatus.Unauthorized));
    }

    [Test]
    public async Task Handle_ShouldAddCommentWithCorrectText()
    {
        // Arrange
        var movie = Movie.Create("Test Movie", null, null, null, null, null, null);
        _movies.Add(movie);

        var command = new CreateCommentCommand
        {
            MovieId = movie.Id,
            Text = "This is an amazing film!"
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var addedComment = movie.Comments.First();
        Assert.That(addedComment.Text, Is.EqualTo("This is an amazing film!"));
    }

    [Test]
    public async Task Handle_ShouldAddCommentWithCorrectUserId()
    {
        // Arrange
        var movie = Movie.Create("Test Movie", null, null, null, null, null, null);
        _movies.Add(movie);
        _userMock.Setup(x => x.Id).Returns("current-user-id");

        var command = new CreateCommentCommand
        {
            MovieId = movie.Id,
            Text = "Great movie!"
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var addedComment = movie.Comments.First();
        Assert.That(addedComment.UserId, Is.EqualTo("current-user-id"));
    }
}
