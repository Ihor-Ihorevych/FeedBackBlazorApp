namespace FB_App.Application.Comments.Commands.RejectComment;

public sealed class RejectCommentCommandValidator : AbstractValidator<RejectCommentCommand>
{
    public RejectCommentCommandValidator()
    {
        RuleFor(x => x.MovieId).NotEmpty().WithMessage("MovieId is required.");
        RuleFor(x => x.CommentId).NotEmpty().WithMessage("CommentId is required.");
    }
}
