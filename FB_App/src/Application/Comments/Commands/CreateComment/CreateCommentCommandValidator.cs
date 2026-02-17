namespace FB_App.Application.Comments.Commands.CreateComment;

public sealed class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(v => v.MovieId)
            .NotEmpty().WithMessage("Movie ID cannot be empty.");

        RuleFor(v => v.Text)
            .NotEmpty().WithMessage("Comment text is required.")
            .MaximumLength(1000).WithMessage("Comment text must not exceed 1000 characters.")
            .MinimumLength(3).WithMessage("Comment text must be at least 3 characters.");
    }
}
