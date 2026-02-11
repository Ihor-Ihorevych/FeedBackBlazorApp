using FB_App.Application.Common.Exceptions;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Security;
using FB_App.Domain.Constants;
using FB_App.Domain.Entities;

using NotFoundException = FB_App.Application.Common.Exceptions.NotFoundException;

namespace FB_App.Application.Comments.Commands.CreateComment;

[Authorize(Roles = Roles.User)]
public record CreateCommentCommand : IRequest<Guid>
{
    public Guid MovieId { get; init; }
    public string Text { get; init; } = string.Empty;
}

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateCommentCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<Guid> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var movie = await _context.Movies
            .FirstOrDefaultAsync(m => m.Id == request.MovieId, cancellationToken);

        if (movie == null)
        {
            throw new NotFoundException(nameof(Movie), request.MovieId.ToString());
        }

        var comment = movie.AddComment(
            _user.Id ?? throw new UnauthorizedAccessException("User ID not found"),
            request.Text
        );

        await _context.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }
}
