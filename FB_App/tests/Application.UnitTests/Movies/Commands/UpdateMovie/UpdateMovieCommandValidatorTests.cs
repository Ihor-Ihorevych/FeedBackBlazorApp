using FB_App.Application.Movies.Commands.UpdateMovie;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace FB_App.Application.UnitTests.Movies.Commands.UpdateMovie;

[TestFixture]
public sealed class UpdateMovieCommandValidatorTests
{
    private UpdateMovieCommandValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new UpdateMovieCommandValidator();
    }

    #region Id Validation Tests

    [Test]
    public void Validate_WithValidId_ShouldNotHaveError()
    {
        var command = new UpdateMovieCommand
        {
            Id = Guid.NewGuid(),
            Title = "Test Movie"
        };

        var result = _validator.TestValidate(command);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.Errors, Has.None.Matches<FluentValidation.Results.ValidationFailure>(
                f => f.PropertyName == nameof(command.Id)));
        }
    }

    [Test]
    public void Validate_WithEmptyId_ShouldHaveError()
    {
        var command = new UpdateMovieCommand
        {
            Id = Guid.Empty,
            Title = "Test Movie"
        };

        var result = _validator.TestValidate(command);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(
                f => f.PropertyName == nameof(command.Id) &&
                     f.ErrorMessage == "Movie ID is required."));
        }
    }

    #endregion

    #region Title Validation Tests

    [Test]
    public void Validate_WithValidTitle_ShouldNotHaveError()
    {
        var command = new UpdateMovieCommand
        {
            Id = Guid.NewGuid(),
            Title = "The Matrix"
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.None.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Title)));
    }

    [Test]
    public void Validate_WithEmptyTitle_ShouldHaveError()
    {
        var command = new UpdateMovieCommand
        {
            Id = Guid.NewGuid(),
            Title = ""
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Title) &&
                 f.ErrorMessage == "Title is required."));
    }

    [Test]
    public void Validate_WithTitleExceeding200Characters_ShouldHaveError()
    {
        var command = new UpdateMovieCommand
        {
            Id = Guid.NewGuid(),
            Title = new string('A', 201)
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Title) &&
                 f.ErrorMessage == "Title must not exceed 200 characters."));
    }

    #endregion

    #region Description Validation Tests

    [Test]
    public void Validate_WithDescriptionExceeding2000Characters_ShouldHaveError()
    {
        var command = new UpdateMovieCommand
        {
            Id = Guid.NewGuid(),
            Title = "Test Movie",
            Description = new string('A', 2001)
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Description) &&
                 f.ErrorMessage == "Description must not exceed 2000 characters."));
    }

    #endregion

    #region ReleaseYear Validation Tests

    [Test]
    public void Validate_WithValidReleaseYear_ShouldNotHaveError()
    {
        var command = new UpdateMovieCommand
        {
            Id = Guid.NewGuid(),
            Title = "Test Movie",
            ReleaseYear = 2024
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.None.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.ReleaseYear)));
    }

    [Test]
    public void Validate_WithReleaseYearBefore1800_ShouldHaveError()
    {
        var command = new UpdateMovieCommand
        {
            Id = Guid.NewGuid(),
            Title = "Test Movie",
            ReleaseYear = 1799
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.ReleaseYear) &&
                 f.ErrorMessage == "Release year must be after 1800."));
    }

    [Test]
    public void Validate_WithReleaseYearTooFarInFuture_ShouldHaveError()
    {
        var command = new UpdateMovieCommand
        {
            Id = Guid.NewGuid(),
            Title = "Test Movie",
            ReleaseYear = DateTime.Now.Year + 10
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.ReleaseYear) &&
                 f.ErrorMessage.Contains("future")));
    }

    #endregion

    #region Director Validation Tests

    [Test]
    public void Validate_WithDirectorExceeding100Characters_ShouldHaveError()
    {
        var command = new UpdateMovieCommand
        {
            Id = Guid.NewGuid(),
            Title = "Test Movie",
            Director = new string('A', 101)
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Director) &&
                 f.ErrorMessage == "Director name must not exceed 100 characters."));
    }

    #endregion

    #region Genre Validation Tests

    [Test]
    public void Validate_WithGenreExceeding50Characters_ShouldHaveError()
    {
        var command = new UpdateMovieCommand
        {
            Id = Guid.NewGuid(),
            Title = "Test Movie",
            Genre = new string('A', 51)
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Genre) &&
                 f.ErrorMessage == "Genre must not exceed 50 characters."));
    }

    #endregion

    #region PosterUrl Validation Tests

    [Test]
    public void Validate_WithPosterUrlExceeding500Characters_ShouldHaveError()
    {
        var command = new UpdateMovieCommand
        {
            Id = Guid.NewGuid(),
            Title = "Test Movie",
            PosterUrl = new string('A', 501)
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.PosterUrl) &&
                 f.ErrorMessage == "Poster URL must not exceed 500 characters."));
    }

    #endregion

    #region Rating Validation Tests

    [Test]
    public void Validate_WithRatingBelowZero_ShouldHaveError()
    {
        var command = new UpdateMovieCommand
        {
            Id = Guid.NewGuid(),
            Title = "Test Movie",
            Rating = -1
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Rating) &&
                 f.ErrorMessage == "Rating must be at least 0."));
    }

    [Test]
    public void Validate_WithRatingAboveTen_ShouldHaveError()
    {
        var command = new UpdateMovieCommand
        {
            Id = Guid.NewGuid(),
            Title = "Test Movie",
            Rating = 10.5D,
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Rating) &&
                 f.ErrorMessage == "Rating must not exceed 10."));
    }

    [Test]
    public void Validate_WithValidRating_ShouldNotHaveError()
    {
        var command = new UpdateMovieCommand
        {
            Id = Guid.NewGuid(),
            Title = "Test Movie",
            Rating = 8.5D,
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.None.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Rating)));
    }

    #endregion

    #region Full Command Validation Tests

    [Test]
    public void Validate_WithFullyValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = new UpdateMovieCommand
        {
            Id = Guid.NewGuid(),
            Title = "Inception",
            Description = "A thief who steals corporate secrets.",
            ReleaseYear = 2010,
            Director = "Christopher Nolan",
            Genre = "Sci-Fi",
            PosterUrl = "https://example.com/inception.jpg",
            Rating = 8.8D,
        };

        var result = _validator.TestValidate(command);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.Errors, Is.Empty);
        }
    }

    [Test]
    public void Validate_WithMultipleErrors_ShouldHaveAllErrors()
    {
        var command = new UpdateMovieCommand
        {
            Id = Guid.Empty,
            Title = "",
            Rating = 15
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.Count.EqualTo(3));
        Assert.That(result.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Id)));
        Assert.That(result.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Title)));
        Assert.That(result.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Rating)));
    }

    #endregion
}
