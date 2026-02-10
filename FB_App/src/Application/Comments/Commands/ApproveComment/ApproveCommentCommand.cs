using FB_App.Application.Common.Exceptions;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Security;
using FB_App.Domain.Constants;
using FB_App.Domain.Entities;
using FB_App.Domain.Enums;
using FB_App.Domain.Events;

using NotFoundException = FB_App.Application.Common.Exceptions.NotFoundException;

namespace FB_App.Application.Comments.Commands.ApproveComment;

[Authorize(Roles = Roles.Administrator)]
public record ApproveCommentCommand(int Id) : IRequest;

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
        var comment = await _context.Comments
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (comment == null)
        {
            throw new NotFoundException(nameof(Comment), request.Id.ToString());
        }

        comment.Status = CommentStatus.Approved;
        comment.ReviewedBy = _user.Id;
        comment.ReviewedAt = DateTimeOffset.UtcNow;

        comment.AddDomainEvent(new CommentApprovedEvent(comment));

        await _context.SaveChangesAsync(cancellationToken);
    }
}
