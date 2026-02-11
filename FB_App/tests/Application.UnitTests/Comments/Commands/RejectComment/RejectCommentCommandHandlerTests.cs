using FB_App.Application.Comments.Commands.RejectComment;
using FB_App.Application.Common.Exceptions;
using FB_App.Application.Common.Interfaces;
using FB_App.Domain.Entities;
using FB_App.Domain.Entities.Values;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace FB_App.Application.UnitTests.Comments.Commands.RejectComment;

[TestFixture]
public class RejectCommentCommandHandlerTests
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
        comment.IsApproved.ShouldBeFalse();
        comment.ReviewedBy.ShouldNotBeNull();
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
    public async Task Handle_WithNonExistentMovie_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new RejectCommentCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _handler.Handle(command, CancellationToken.None));
        
        exception.Message.ShouldContain("Movie");
    }

    [Test]
    public async Task Handle_WithNonExistentComment_ShouldThrowNotFoundException()
    {
        // Arrange
        var movie = Movie.Create("Test Movie", null, null, null, null, null, null);
        _movies.Add(movie);

        var command = new RejectCommentCommand(movie.Id, Guid.NewGuid());

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _handler.Handle(command, CancellationToken.None));
        
        exception.Message.ShouldContain("Comment");
    }

    [Test]
    public async Task Handle_WithNullUserId_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var movie = Movie.Create("Test Movie", null, null, null, null, null, null);
        movie.AddComment("user-123", "Great movie!");
        var comment = movie.Comments.First();
        _movies.Add(movie);
        _userMock.Setup(x => x.Id).Returns((string?)null);

        var command = new RejectCommentCommand(movie.Id, comment.Id);

        // Act & Assert
        await Should.ThrowAsync<UnauthorizedAccessException>(async () =>
            await _handler.Handle(command, CancellationToken.None));
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
        comment.ReviewedBy.ShouldBe("reviewer-admin-id");
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

// Helper classes for async DbSet mocking
internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object? Execute(System.Linq.Expressions.Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(
                name: nameof(IQueryProvider.Execute),
                genericParameterCount: 1,
                types: new[] { typeof(System.Linq.Expressions.Expression) })!
            .MakeGenericMethod(resultType)
            .Invoke(this, new[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    { }

    public TestAsyncEnumerable(System.Linq.Expressions.Expression expression)
        : base(expression)
    { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
}
