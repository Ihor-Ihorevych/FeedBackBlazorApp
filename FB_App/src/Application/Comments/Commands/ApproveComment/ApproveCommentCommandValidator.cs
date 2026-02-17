namespace FB_App.Application.Comments.Commands.ApproveComment;

public sealed class ApproveCommentCommandValidator : AbstractValidator<ApproveCommentCommand>
{
    public ApproveCommentCommandValidator()
    {
        RuleFor(x => x.MovieId).NotEmpty().WithMessage("MovieId is required.");
        RuleFor(x => x.CommentId).NotEmpty().WithMessage("CommentId is required.");
    }
}
