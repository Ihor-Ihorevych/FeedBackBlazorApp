using FB_App.Application.Comments.Commands.CreateComment;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace FB_App.Application.UnitTests.Comments.Commands.CreateComment;

[TestFixture]
public class CreateCommentCommandValidatorTests
{
    private CreateCommentCommandValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new CreateCommentCommandValidator();
    }

    #region MovieId Validation Tests

    [Test]
    public void Validate_WithValidMovieId_ShouldNotHaveError()
    {
        var command = new CreateCommentCommand
        {
            MovieId = Guid.NewGuid(),
            Text = "Great movie!"
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.MovieId);
    }

    [Test]
    public void Validate_WithEmptyMovieId_ShouldHaveError()
    {
        var command = new CreateCommentCommand
        {
            MovieId = Guid.Empty,
            Text = "Great movie!"
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.MovieId)
            .WithErrorMessage("Movie ID cannot be empty.");
    }

    #endregion

    #region Text Validation Tests

    [Test]
    public void Validate_WithValidText_ShouldNotHaveError()
    {
        var command = new CreateCommentCommand
        {
            MovieId = Guid.NewGuid(),
            Text = "This is a great movie with amazing visuals!"
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Text);
    }

    [Test]
    public void Validate_WithEmptyText_ShouldHaveError()
    {
        var command = new CreateCommentCommand
        {
            MovieId = Guid.NewGuid(),
            Text = ""
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Text)
            .WithErrorMessage("Comment text is required.");
    }

    [Test]
    public void Validate_WithTextTooShort_ShouldHaveError()
    {
        var command = new CreateCommentCommand
        {
            MovieId = Guid.NewGuid(),
            Text = "Hi"
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Text)
            .WithErrorMessage("Comment text must be at least 3 characters.");
    }

    [Test]
    public void Validate_WithTextExactly3Characters_ShouldNotHaveError()
    {
        var command = new CreateCommentCommand
        {
            MovieId = Guid.NewGuid(),
            Text = "Wow"
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Text);
    }

    [Test]
    public void Validate_WithTextExceeding1000Characters_ShouldHaveError()
    {
        var command = new CreateCommentCommand
        {
            MovieId = Guid.NewGuid(),
            Text = new string('A', 1001)
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Text)
            .WithErrorMessage("Comment text must not exceed 1000 characters.");
    }

    [Test]
    public void Validate_WithTextExactly1000Characters_ShouldNotHaveError()
    {
        var command = new CreateCommentCommand
        {
            MovieId = Guid.NewGuid(),
            Text = new string('A', 1000)
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Text);
    }

    [Test]
    public void Validate_WithWhitespaceOnlyText_ShouldHaveError()
    {
        var command = new CreateCommentCommand
        {
            MovieId = Guid.NewGuid(),
            Text = "   "
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Text);
    }

    #endregion

    #region Full Command Validation Tests

    [Test]
    public void Validate_WithFullyValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = new CreateCommentCommand
        {
            MovieId = Guid.NewGuid(),
            Text = "Amazing movie! The plot twists were incredible."
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_WithMultipleErrors_ShouldHaveAllErrors()
    {
        var command = new CreateCommentCommand
        {
            MovieId = Guid.Empty,
            Text = ""
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.MovieId);
        result.ShouldHaveValidationErrorFor(x => x.Text);
    }

    #endregion
}
