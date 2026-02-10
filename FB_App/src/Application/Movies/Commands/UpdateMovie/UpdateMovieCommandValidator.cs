namespace FB_App.Application.Movies.Commands.UpdateMovie;

public class UpdateMovieCommandValidator : AbstractValidator<UpdateMovieCommand>
{
    public UpdateMovieCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(v => v.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(v => v.ReleaseYear)
            .GreaterThan(1800).WithMessage("Release year must be after 1800.")
            .LessThanOrEqualTo(DateTime.Now.Year + 5).WithMessage("Release year cannot be too far in the future.")
            .When(v => v.ReleaseYear.HasValue);

        RuleFor(v => v.Director)
            .MaximumLength(100).WithMessage("Director name must not exceed 100 characters.");

        RuleFor(v => v.Genre)
            .MaximumLength(50).WithMessage("Genre must not exceed 50 characters.");

        RuleFor(v => v.PosterUrl)
            .MaximumLength(500).WithMessage("Poster URL must not exceed 500 characters.");

        RuleFor(v => v.Rating)
            .GreaterThanOrEqualTo(0).WithMessage("Rating must be at least 0.")
            .LessThanOrEqualTo(10).WithMessage("Rating must not exceed 10.")
            .When(v => v.Rating.HasValue);
    }
}
