using Ardalis.Result;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Security;
using FB_App.Domain.Constants;
using FB_App.Domain.Entities;

namespace FB_App.Application.Comments.Commands.CreateComment;

[Authorize(Roles = $"{Roles.Administrator},{Roles.User}")]
public sealed record CreateCommentCommand : IRequest<Result<Guid>>
{
    public Guid MovieId { get; init; }
    public string Text { get; init; } = string.Empty;
}

public sealed class CreateCommentCommandHandler(IApplicationDbContext context, IUser user) : IRequestHandler<CreateCommentCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IUser _user = user;

    public async Task<Result<Guid>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;
        if (string.IsNullOrEmpty(userId))
        {
            return Result<Guid>.Unauthorized();
        }

        var movie = await _context.Movies
            .FirstOrDefaultAsync(m => m.Id == request.MovieId, cancellationToken);

        if (movie == null)
        {
            return Result<Guid>.NotFound($"{nameof(Movie)} ({request.MovieId}) was not found.");
        }
       

        var comment = movie.AddComment(userId, request.Text);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(comment.Id);
    }
}
