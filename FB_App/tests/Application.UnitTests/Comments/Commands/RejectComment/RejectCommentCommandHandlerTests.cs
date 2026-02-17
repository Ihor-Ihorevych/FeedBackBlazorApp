using Ardalis.Result;
using FB_App.Application.Comments.Commands.RejectComment;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.UnitTests.Common.Testing;
using FB_App.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace FB_App.Application.UnitTests.Comments.Commands.RejectComment;

[TestFixture]
public sealed class RejectCommentCommandHandlerTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IUser> _userMock = null!;
    private RejectCommentCommandHandler _handler = null!;
    private List<Movie> _movies = null!;

    [SetUp]
    public void SetUp()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _userMock = new Mock<IUser>();
        _movies = new List<Movie>();

        var moviesDbSetMock = CreateMockDbSet(_movies);
        _contextMock.Setup(x => x.Movies).Returns(moviesDbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _userMock.Setup(x => x.Id).Returns("admin-user-123");

        _handler = new RejectCommentCommandHandler(_contextMock.Object, _userMock.Object);
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
    public async Task Handle_WithValidMovieAndComment_ShouldRejectComment()
    {
        // Arrange
        var movie = Movie.Create("Test Movie", null, null, null, null, null, null);
        movie.AddComment("user-123", "Great movie!");
        var comment = movie.Comments.First();
        _movies.Add(movie);

        var command = new RejectCommentCommand(movie.Id, comment.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(comment.IsApproved, Is.False);
        Assert.That(comment.ReviewedBy, Is.Not.Null);
    }

    [Test]
    public async Task Handle_WithValidRejection_ShouldSaveChanges()
    {
        // Arrange
        var movie = Movie.Create("Test Movie", null, null, null, null, null, null);
        movie.AddComment("user-123", "Great movie!");
        var comment = movie.Comments.First();
        _movies.Add(movie);

        var command = new RejectCommentCommand(movie.Id, comment.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_WithNonExistentMovie_ShouldReturnNotFoundResult()
    {
        // Arrange
        var command = new RejectCommentCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That(result.Errors.Single(), Does.Contain("Movie"));
    }

    [Test]
    public async Task Handle_WithNonExistentComment_ShouldReturnNotFoundResult()
    {
        // Arrange
        var movie = Movie.Create("Test Movie", null, null, null, null, null, null);
        _movies.Add(movie);

        var command = new RejectCommentCommand(movie.Id, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That(result.Errors.Single(), Does.Contain("Comment"));
    }

    [Test]
    public async Task Handle_WithNullUserId_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        var movie = Movie.Create("Test Movie", null, null, null, null, null, null);
        movie.AddComment("user-123", "Great movie!");
        var comment = movie.Comments.First();
        _movies.Add(movie);
        _userMock.Setup(x => x.Id).Returns((string?)null);

        var command = new RejectCommentCommand(movie.Id, comment.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Status, Is.EqualTo(ResultStatus.Unauthorized));
    }

    [Test]
    public async Task Handle_ShouldSetReviewerIdFromCurrentUser()
    {
        // Arrange
        var movie = Movie.Create("Test Movie", null, null, null, null, null, null);
        movie.AddComment("user-123", "Great movie!");
        var comment = movie.Comments.First();
        _movies.Add(movie);
        _userMock.Setup(x => x.Id).Returns("reviewer-admin-id");

        var command = new RejectCommentCommand(movie.Id, comment.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(comment.ReviewedBy, Is.EqualTo("reviewer-admin-id"));
    }

    [Test]
    public async Task Handle_WithCancellationToken_ShouldPassToSaveChanges()
    {
        // Arrange
        var movie = Movie.Create("Test Movie", null, null, null, null, null, null);
        movie.AddComment("user-123", "Great movie!");
        var comment = movie.Comments.First();
        _movies.Add(movie);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        var command = new RejectCommentCommand(movie.Id, comment.Id);

        // Act
        await _handler.Handle(command, token);

        // Assert
        _contextMock.Verify(x => x.SaveChangesAsync(token), Times.Once);
    }
}
