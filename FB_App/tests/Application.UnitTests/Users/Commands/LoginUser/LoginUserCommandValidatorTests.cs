using FB_App.Application.Users.Commands.LoginUser;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace FB_App.Application.UnitTests.Users.Commands.LoginUser;

[TestFixture]
public class LoginUserCommandValidatorTests
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

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
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

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
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

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must be a valid email address.");
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

        result.ShouldHaveValidationErrorFor(x => x.Email);
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

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
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

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required.");
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

        result.ShouldHaveValidationErrorFor(x => x.Password);
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

        result.ShouldNotHaveAnyValidationErrors();
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

        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    #endregion
}
