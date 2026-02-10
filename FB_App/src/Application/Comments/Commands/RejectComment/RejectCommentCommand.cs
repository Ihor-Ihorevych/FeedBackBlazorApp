using FB_App.Application.Common.Exceptions;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Security;
using FB_App.Domain.Constants;
using FB_App.Domain.Entities;

using NotFoundException = FB_App.Application.Common.Exceptions.NotFoundException;

namespace FB_App.Application.Comments.Commands.RejectComment;

[Authorize(Roles = Roles.Administrator)]
public record RejectCommentCommand(int MovieId, int CommentId) : IRequest;

public class RejectCommentCommandHandler : IRequestHandler<RejectCommentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public RejectCommentCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(RejectCommentCommand request, CancellationToken cancellationToken)
    {
        var movie = await _context.Movies
            .FirstOrDefaultAsync(m => m.Id == request.MovieId, cancellationToken);

        if (movie == null)
        {
            throw new NotFoundException(nameof(Movie), request.MovieId.ToString());
        }

        var comment = movie.GetComment(request.CommentId);
        if (comment == null)
        {
            throw new NotFoundException(nameof(Comment), request.CommentId.ToString());
        }

        var userId = _user.Id ?? throw new UnauthorizedAccessException("User ID not found");
        movie.RejectComment(request.CommentId, userId);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
