using FB_App.Application.Common.Exceptions;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Security;
using FB_App.Domain.Constants;
using FB_App.Domain.Entities;
using FB_App.Domain.Enums;
using FB_App.Domain.Events;

using NotFoundException = FB_App.Application.Common.Exceptions.NotFoundException;

namespace FB_App.Application.Comments.Commands.CreateComment;

[Authorize(Roles = Roles.User)]
public record CreateCommentCommand : IRequest<int>
{
    public int MovieId { get; init; }
    public string Text { get; init; } = string.Empty;
}

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateCommentCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<int> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var movieExists = await _context.Movies
            .AnyAsync(m => m.Id == request.MovieId, cancellationToken);

        if (!movieExists)
        {
            throw new NotFoundException(nameof(Movie), request.MovieId.ToString());
        }

        var comment = new Comment
        {
            MovieId = request.MovieId,
            UserId = _user.Id ?? throw new UnauthorizedAccessException("User ID not found"),
            Text = request.Text,
            Status = CommentStatus.Pending
        };

        comment.AddDomainEvent(new CommentCreatedEvent(comment));

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }
}
