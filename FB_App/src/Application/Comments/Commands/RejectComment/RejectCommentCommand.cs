using Ardalis.Result;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Security;
using FB_App.Domain.Constants;
using FB_App.Domain.Entities;
using FB_App.Domain.Entities.Values;

namespace FB_App.Application.Comments.Commands.RejectComment;

[Authorize(Roles = Roles.Administrator)]
public sealed record RejectCommentCommand(Guid MovieId, Guid CommentId) : IRequest<Result>;
public sealed class RejectCommentCommandHandler(IApplicationDbContext context, IUser user) : IRequestHandler<RejectCommentCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IUser _user = user;

    public async Task<Result> Handle(RejectCommentCommand request, CancellationToken cancellationToken)
    {
        var movie = await _context.Movies
            .Include(m => m.Comments)
            .FirstOrDefaultAsync(m => m.Id == request.MovieId, cancellationToken);

        if (movie == null)
        {
            return Result.NotFound($"{nameof(Movie)} ({request.MovieId}) was not found.");
        }

        var comment = movie.GetComment((CommentId)request.CommentId);
        if (comment == null)
        {
            return Result.NotFound($"{nameof(Comment)} ({request.CommentId}) was not found.");
        }

        var userId = _user.Id;
        if (string.IsNullOrEmpty(userId))
        {
            return Result.Unauthorized();
        }

        movie.RejectComment((CommentId)request.CommentId, userId);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
