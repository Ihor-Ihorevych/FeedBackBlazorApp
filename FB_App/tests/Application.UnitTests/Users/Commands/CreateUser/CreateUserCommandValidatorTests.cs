using FB_App.Application.Users.Commands.CreateUser;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace FB_App.Application.UnitTests.Users.Commands.CreateUser;

[TestFixture]
public class CreateUserCommandValidatorTests
{
    private CreateUserCommandValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new CreateUserCommandValidator();
    }

    #region Email Validation Tests

    [Test]
    public void Validate_WithValidEmail_ShouldNotHaveEmailError()
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "ValidPass1!"
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.None.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Email)));
    }

    [Test]
    public void Validate_WithEmptyEmail_ShouldHaveEmailError()
    {
        var command = new CreateUserCommand
        {
            Email = "",
            Password = "ValidPass1!"
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Email) &&
                 f.ErrorMessage == "Email is required."));
    }

    [Test]
    public void Validate_WithInvalidEmailFormat_ShouldHaveEmailError()
    {
        var command = new CreateUserCommand
        {
            Email = "not-an-email",
            Password = "ValidPass1!"
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
        var command = new CreateUserCommand
        {
            Email = invalidEmail,
            Password = "ValidPass1!"
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
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "ValidPass1!"
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.None.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Password)));
    }

    [Test]
    public void Validate_WithEmptyPassword_ShouldHavePasswordError()
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = ""
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Password)));
    }

    [Test]
    public void Validate_WithPasswordTooShort_ShouldHavePasswordError()
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "Ab1!"
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Password)));
    }

    [Test]
    public void Validate_WithPasswordNoUppercase_ShouldHavePasswordError()
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "validpass1!"
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Password)));
    }

    [Test]
    public void Validate_WithPasswordNoLowercase_ShouldHavePasswordError()
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "VALIDPASS1!"
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Password)));
    }

    [Test]
    public void Validate_WithPasswordNoSpecialCharacter_ShouldHavePasswordError()
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "ValidPass1"
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Password)));
    }

    [TestCase("Password1!")]
    [TestCase("MyP@ssw0rd")]
    [TestCase("Str0ng!Pass")]
    [TestCase("Complex#123")]
    public void Validate_WithVariousValidPasswords_ShouldNotHavePasswordError(string validPassword)
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = validPassword
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.Errors, Has.None.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Password)));
    }

    #endregion

    #region Full Command Validation Tests

    [Test]
    public void Validate_WithValidCommand_ShouldBeValid()
    {
        var command = new CreateUserCommand
        {
            Email = "user@example.com",
            Password = "SecurePass1!"
        };

        var result = _validator.TestValidate(command);

        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public void Validate_WithAllFieldsInvalid_ShouldHaveMultipleErrors()
    {
        var command = new CreateUserCommand
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
