using FB_App.Application.Common.Exceptions;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Security;
using FB_App.Domain.Constants;
using FB_App.Domain.Entities;

using NotFoundException = FB_App.Application.Common.Exceptions.NotFoundException;

namespace FB_App.Application.Comments.Commands.ApproveComment;

[Authorize(Roles = Roles.Administrator)]
public record ApproveCommentCommand(int MovieId, int CommentId) : IRequest;

public class ApproveCommentCommandHandler : IRequestHandler<ApproveCommentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public ApproveCommentCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(ApproveCommentCommand request, CancellationToken cancellationToken)
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
        movie.ApproveComment(request.CommentId, userId);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
