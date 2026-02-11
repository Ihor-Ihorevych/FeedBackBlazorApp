using FB_App.Application.Movies.Commands.CreateMovie;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace FB_App.Application.UnitTests.Movies.Commands.CreateMovie;

[TestFixture]
public class CreateMovieCommandValidatorTests
{
    private CreateMovieCommandValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new CreateMovieCommandValidator();
    }

    #region Title Validation Tests

    [Test]
    public void Validate_WithValidTitle_ShouldNotHaveError()
    {
        var command = new CreateMovieCommand { Title = "The Matrix" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Test]
    public void Validate_WithEmptyTitle_ShouldHaveError()
    {
        var command = new CreateMovieCommand { Title = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required.");
    }

    [Test]
    public void Validate_WithTitleExceeding200Characters_ShouldHaveError()
    {
        var command = new CreateMovieCommand { Title = new string('A', 201) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must not exceed 200 characters.");
    }

    [Test]
    public void Validate_WithTitleExactly200Characters_ShouldNotHaveError()
    {
        var command = new CreateMovieCommand { Title = new string('A', 200) };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    #endregion

    #region Description Validation Tests

    [Test]
    public void Validate_WithValidDescription_ShouldNotHaveError()
    {
        var command = new CreateMovieCommand 
        { 
            Title = "Test Movie",
            Description = "A great movie about testing." 
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Test]
    public void Validate_WithDescriptionExceeding2000Characters_ShouldHaveError()
    {
        var command = new CreateMovieCommand 
        { 
            Title = "Test Movie",
            Description = new string('A', 2001) 
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must not exceed 2000 characters.");
    }

    [Test]
    public void Validate_WithNullDescription_ShouldNotHaveError()
    {
        var command = new CreateMovieCommand 
        { 
            Title = "Test Movie",
            Description = null 
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    #endregion

    #region ReleaseYear Validation Tests

    [Test]
    public void Validate_WithValidReleaseYear_ShouldNotHaveError()
    {
        var command = new CreateMovieCommand 
        { 
            Title = "Test Movie",
            ReleaseYear = 2024 
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.ReleaseYear);
    }

    [Test]
    public void Validate_WithReleaseYearBefore1800_ShouldHaveError()
    {
        var command = new CreateMovieCommand 
        { 
            Title = "Test Movie",
            ReleaseYear = 1799 
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ReleaseYear)
            .WithErrorMessage("Release year must be after 1800.");
    }

    [Test]
    public void Validate_WithReleaseYearTooFarInFuture_ShouldHaveError()
    {
        var command = new CreateMovieCommand 
        { 
            Title = "Test Movie",
            ReleaseYear = DateTime.Now.Year + 10 
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ReleaseYear)
            .WithErrorMessage("Release year cannot be too far in the future.");
    }

    [Test]
    public void Validate_WithNullReleaseYear_ShouldNotHaveError()
    {
        var command = new CreateMovieCommand 
        { 
            Title = "Test Movie",
            ReleaseYear = null 
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.ReleaseYear);
    }

    #endregion

    #region Director Validation Tests

    [Test]
    public void Validate_WithValidDirector_ShouldNotHaveError()
    {
        var command = new CreateMovieCommand 
        { 
            Title = "Test Movie",
            Director = "Christopher Nolan" 
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Director);
    }

    [Test]
    public void Validate_WithDirectorExceeding100Characters_ShouldHaveError()
    {
        var command = new CreateMovieCommand 
        { 
            Title = "Test Movie",
            Director = new string('A', 101) 
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Director)
            .WithErrorMessage("Director name must not exceed 100 characters.");
    }

    #endregion

    #region Genre Validation Tests

    [Test]
    public void Validate_WithValidGenre_ShouldNotHaveError()
    {
        var command = new CreateMovieCommand 
        { 
            Title = "Test Movie",
            Genre = "Action" 
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Genre);
    }

    [Test]
    public void Validate_WithGenreExceeding50Characters_ShouldHaveError()
    {
        var command = new CreateMovieCommand 
        { 
            Title = "Test Movie",
            Genre = new string('A', 51) 
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Genre)
            .WithErrorMessage("Genre must not exceed 50 characters.");
    }

    #endregion

    #region PosterUrl Validation Tests

    [Test]
    public void Validate_WithValidPosterUrl_ShouldNotHaveError()
    {
        var command = new CreateMovieCommand 
        { 
            Title = "Test Movie",
            PosterUrl = "https://example.com/poster.jpg" 
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.PosterUrl);
    }

    [Test]
    public void Validate_WithPosterUrlExceeding500Characters_ShouldHaveError()
    {
        var command = new CreateMovieCommand 
        { 
            Title = "Test Movie",
            PosterUrl = new string('A', 501) 
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PosterUrl)
            .WithErrorMessage("Poster URL must not exceed 500 characters.");
    }

    #endregion

    #region Rating Validation Tests

    [Test]
    public void Validate_WithValidRating_ShouldNotHaveError()
    {
        var command = new CreateMovieCommand 
        { 
            Title = "Test Movie",
            Rating = 8.5 
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Rating);
    }

    [Test]
    public void Validate_WithRatingBelowZero_ShouldHaveError()
    {
        var command = new CreateMovieCommand 
        { 
            Title = "Test Movie",
            Rating = -1 
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Rating)
            .WithErrorMessage("Rating must be at least 0.");
    }

    [Test]
    public void Validate_WithRatingAboveTen_ShouldHaveError()
    {
        var command = new CreateMovieCommand 
        { 
            Title = "Test Movie",
            Rating = 10.5 
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Rating)
            .WithErrorMessage("Rating must not exceed 10.");
    }

    [Test]
    public void Validate_WithRatingExactlyZero_ShouldNotHaveError()
    {
        var command = new CreateMovieCommand 
        { 
            Title = "Test Movie",
            Rating = 0 
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Rating);
    }

    [Test]
    public void Validate_WithRatingExactlyTen_ShouldNotHaveError()
    {
        var command = new CreateMovieCommand 
        { 
            Title = "Test Movie",
            Rating = 10 
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Rating);
    }

    #endregion

    #region Full Command Validation Tests

    [Test]
    public void Validate_WithFullyValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = new CreateMovieCommand
        {
            Title = "Inception",
            Description = "A thief who steals corporate secrets through dream-sharing technology.",
            ReleaseYear = 2010,
            Director = "Christopher Nolan",
            Genre = "Sci-Fi",
            PosterUrl = "https://example.com/inception.jpg",
            Rating = 8.8
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}
