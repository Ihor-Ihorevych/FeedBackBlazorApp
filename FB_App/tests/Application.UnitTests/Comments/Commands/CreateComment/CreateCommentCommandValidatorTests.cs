using FB_App.Application.Comments.Commands.CreateComment;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace FB_App.Application.UnitTests.Comments.Commands.CreateComment;

[TestFixture]
public sealed class CreateCommentCommandValidatorTests
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
        // Arrange
        var command = new CreateCommentCommand
        {
            MovieId = Guid.NewGuid(),
            Text = "Great movie!"
        };

        // Act
        var result = _validator.TestValidate(command);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.Errors, Has.None.Matches<FluentValidation.Results.ValidationFailure>(
                f => f.PropertyName == nameof(command.MovieId)));
        }
    }

    [Test]
    public void Validate_WithEmptyMovieId_ShouldHaveError()
    {
        // Arrange
        var command = new CreateCommentCommand
        {
            MovieId = Guid.Empty,
            Text = "Great movie!"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        Assert.That(result.IsValid, Is.False);
    }

    #endregion

    #region Text Validation Tests

    [Test]
    public void Validate_WithValidText_ShouldNotHaveError()
    {
        // Arrange
        var command = new CreateCommentCommand
        {
            MovieId = Guid.NewGuid(),
            Text = "This is a great movie with amazing visuals!"
        };

        // Act
        var result = _validator.TestValidate(command);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.Errors, Has.None.Matches<FluentValidation.Results.ValidationFailure>(
                f => f.PropertyName == nameof(command.Text)));
        }
    }

    [Test]
    public void Validate_WithEmptyText_ShouldHaveError()
    {
        // Arrange
        var command = new CreateCommentCommand
        {
            MovieId = Guid.NewGuid(),
            Text = ""
        };

        // Act
        var result = _validator.TestValidate(command);

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Validate_WithTextTooShort_ShouldHaveError()
    {
        // Arrange
        var command = new CreateCommentCommand
        {
            MovieId = Guid.NewGuid(),
            Text = "Hi"
        };

        // Act
        var result = _validator.TestValidate(command);

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Validate_WithTextExactly3Characters_ShouldNotHaveError()
    {
        // Arrange
        var command = new CreateCommentCommand
        {
            MovieId = Guid.NewGuid(),
            Text = "Wow"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        Assert.That(result.Errors, Has.None.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Text)));
    }

    [Test]
    public void Validate_WithTextExceeding1000Characters_ShouldHaveError()
    {
        // Arrange
        var command = new CreateCommentCommand
        {
            MovieId = Guid.NewGuid(),
            Text = new string('A', 1001)
        };

        // Act
        var result = _validator.TestValidate(command);

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Validate_WithTextExactly1000Characters_ShouldNotHaveError()
    {
        // Arrange
        var command = new CreateCommentCommand
        {
            MovieId = Guid.NewGuid(),
            Text = new string('A', 1000)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        Assert.That(result.Errors, Has.None.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Text)));
    }

    [Test]
    public void Validate_WithWhitespaceOnlyText_ShouldHaveError()
    {
        // Arrange
        var command = new CreateCommentCommand
        {
            MovieId = Guid.NewGuid(),
            Text = "   "
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        Assert.That(result.IsValid, Is.False);
    }

    #endregion

    #region Full Command Validation Tests

    [Test]
    public void Validate_WithFullyValidCommand_ShouldNotHaveAnyErrors()
    {
        // Arrange
        var command = new CreateCommentCommand
        {
            MovieId = Guid.NewGuid(),
            Text = "Amazing movie! The plot twists were incredible."
        };

        // Act
        var result = _validator.TestValidate(command);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.Errors, Is.Empty);
        }
    }

    [Test]
    public void Validate_WithMultipleErrors_ShouldHaveAllErrors()
    {
        // Arrange
        var command = new CreateCommentCommand
        {
            MovieId = Guid.Empty,
            Text = ""
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.MovieId)));
        Assert.That(result.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Text)));
    }



    #endregion
}
