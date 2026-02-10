using FB_App.Application.Common.Exceptions;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Security;
using FB_App.Domain.Constants;
using FB_App.Domain.Entities;
using FB_App.Domain.Enums;
using FB_App.Domain.Events;

using NotFoundException = FB_App.Application.Common.Exceptions.NotFoundException;

namespace FB_App.Application.Comments.Commands.RejectComment;

[Authorize(Roles = Roles.Administrator)]
public record RejectCommentCommand(int Id) : IRequest;

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
        var comment = await _context.Comments
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (comment == null)
        {
            throw new NotFoundException(nameof(Comment), request.Id.ToString());
        }

        comment.Status = CommentStatus.Rejected;
        comment.ReviewedBy = _user.Id;
        comment.ReviewedAt = DateTimeOffset.UtcNow;

        comment.AddDomainEvent(new CommentRejectedEvent(comment));

        await _context.SaveChangesAsync(cancellationToken);
    }
}
