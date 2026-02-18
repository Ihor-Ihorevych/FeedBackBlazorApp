using Ardalis.Result;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Security;
using FB_App.Domain.Constants;
using FB_App.Domain.Entities;
using FB_App.Domain.Entities.Values;

namespace FB_App.Application.Comments.Commands.ApproveComment;

[Authorize(Roles = Roles.Administrator)]
public record ApproveCommentCommand(Guid MovieId, Guid CommentId) : IRequest<Result>;


public sealed class ApproveCommentCommandHandler(IApplicationDbContext context, IUser user) : IRequestHandler<ApproveCommentCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IUser _user = user;

    public async Task<Result> Handle(ApproveCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;
        if (string.IsNullOrEmpty(userId))
        {
            return Result.Unauthorized();
        }

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


        movie.ApproveComment((CommentId)request.CommentId, userId);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
