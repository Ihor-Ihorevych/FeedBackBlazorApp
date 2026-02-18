using FB_App.Application.Users.Commands.LoginUser;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace FB_App.Application.UnitTests.Users.Commands.LoginUser;

[TestFixture]
public sealed class LoginUserCommandValidatorTests
{
    private LoginUserCommandValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new LoginUserCommandValidator();
    }

    #region Email Validation Tests

    [Test]
    public void Validate_WithValidEmail_ShouldNotHaveEmailError()
    {
        var command = new LoginUserCommand
        {
            Email = "test@example.com",
            Password = "anypassword"
        };

        var result = _validator.TestValidate(command);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.Errors, Has.None.Matches<FluentValidation.Results.ValidationFailure>(
                f => f.PropertyName == nameof(command.Email)));
        }
    }

    [Test]
    public void Validate_WithEmptyEmail_ShouldHaveEmailError()
    {
        var command = new LoginUserCommand
        {
            Email = "",
            Password = "anypassword"
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Email) &&
                 f.ErrorMessage == "Email is required."));
    }

    [Test]
    public void Validate_WithInvalidEmailFormat_ShouldHaveEmailError()
    {
        var command = new LoginUserCommand
        {
            Email = "not-an-email",
            Password = "anypassword"
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Email) &&
                 f.ErrorMessage == "Email must be a valid email address."));
    }

    [TestCase("test@")]
    [TestCase("@example.com")]
    [TestCase("test")]
    public void Validate_WithVariousInvalidEmails_ShouldHaveEmailError(string invalidEmail)
    {
        var command = new LoginUserCommand
        {
            Email = invalidEmail,
            Password = "anypassword"
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Email)));
    }

    #endregion

    #region Password Validation Tests

    [Test]
    public void Validate_WithValidPassword_ShouldNotHavePasswordError()
    {
        var command = new LoginUserCommand
        {
            Email = "test@example.com",
            Password = "anypassword"
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.None.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Password)));
    }

    [Test]
    public void Validate_WithEmptyPassword_ShouldHavePasswordError()
    {
        var command = new LoginUserCommand
        {
            Email = "test@example.com",
            Password = ""
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Password) &&
                 f.ErrorMessage == "Password is required."));
    }

    [Test]
    public void Validate_WithWhitespacePassword_ShouldHavePasswordError()
    {
        var command = new LoginUserCommand
        {
            Email = "test@example.com",
            Password = "   "
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Password)));
    }

    #endregion

    #region Full Command Validation Tests

    [Test]
    public void Validate_WithValidCommand_ShouldBeValid()
    {
        var command = new LoginUserCommand
        {
            Email = "user@example.com",
            Password = "password123"
        };

        var result = _validator.TestValidate(command);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.Errors, Is.Empty);
        }
    }

    [Test]
    public void Validate_WithAllFieldsEmpty_ShouldHaveMultipleErrors()
    {
        var command = new LoginUserCommand
        {
            Email = "",
            Password = ""
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.Count.GreaterThanOrEqualTo(2));
        Assert.That(result.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Email)));
        Assert.That(result.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Password)));
    }

    #endregion
}
