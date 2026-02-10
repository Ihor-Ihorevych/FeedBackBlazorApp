namespace FB_App.Application.Comments.Commands.CreateComment;

public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(v => v.MovieId)
            .GreaterThan(0).WithMessage("Movie ID must be greater than 0.");

        RuleFor(v => v.Text)
            .NotEmpty().WithMessage("Comment text is required.")
            .MaximumLength(1000).WithMessage("Comment text must not exceed 1000 characters.")
            .MinimumLength(3).WithMessage("Comment text must be at least 3 characters.");
    }
}
