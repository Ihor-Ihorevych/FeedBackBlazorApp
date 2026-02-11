using Ardalis.Result;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Security;
using FB_App.Domain.Constants;
using FB_App.Domain.Entities;
using FB_App.Domain.Entities.Values;

namespace FB_App.Application.Comments.Commands.RejectComment;

[Authorize(Roles = Roles.Administrator)]
public record RejectCommentCommand(Guid MovieId, Guid CommentId) : IRequest<Result>;

public class RejectCommentCommandHandler : IRequestHandler<RejectCommentCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public RejectCommentCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<Result> Handle(RejectCommentCommand request, CancellationToken cancellationToken)
    {
        var movie = await _context.Movies
            .Include(m => m.Comments.Where(c => c.MovieId == m.Id))
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
