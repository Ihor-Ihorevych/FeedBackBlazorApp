using FB_App.Application.Common.Interfaces;
using FB_App.Application.Users.Commands.CreateUser;
using FluentValidation.TestHelper;
using Moq;
using NUnit.Framework;

namespace FB_App.Application.UnitTests.Users.Commands.CreateUser;

[TestFixture]
public class CreateUserCommandValidatorTests
{
    private CreateUserCommandValidator _validator = null!;


    [SetUp]
    public void SetUp()
    {
        var mock = new Mock<IIdentityService>();
        // Setup mock behavior for GetUserNameAsync
        mock.Setup(s => s.GetUserNameAsync(It.IsAny<string>())).ReturnsAsync((string?)null);
        _validator = new CreateUserCommandValidator(mock.Object);
    }

    #region Email Validation Tests

    [Test]
    public async Task Validate_WithValidEmail_ShouldNotHaveEmailError()
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "ValidPass1!",
            UserName = "test-user"
        };

        var result = await _validator.TestValidateAsync(command);

        Assert.That(result.Errors, Has.None.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Email)));
    }

    [Test]
    public async Task Validate_WithEmptyEmail_ShouldHaveEmailError()
    {
        var command = new CreateUserCommand
        {
            Email = "",
            Password = "ValidPass1!",
            UserName = "test-user"
        };

        var result = await _validator.TestValidateAsync(command);

        Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Email) &&
                 f.ErrorMessage == "Email is required."));
    }

    [Test]
    public async Task Validate_WithInvalidEmailFormat_ShouldHaveEmailError()
    {
        var command = new CreateUserCommand
        {
            Email = "not-an-email",
            Password = "ValidPass1!",
            UserName = "test-user"
        };

        var result = await _validator.TestValidateAsync(command);

        Assert.That(result.Errors, Has.One.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Email) &&
                 f.ErrorMessage == "Email must be a valid email address."));
    }

    [TestCase("test@")]
    [TestCase("@example.com")]
    [TestCase("test")]
    public async Task Validate_WithVariousInvalidEmails_ShouldHaveEmailError(string invalidEmail)
    {
        var command = new CreateUserCommand
        {
            Email = invalidEmail,
            Password = "ValidPass1!",
            UserName = "test-user"
        };

        var result = await _validator.TestValidateAsync(command);

        Assert.That(result.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Email)));
    }

    #endregion

    #region Password Validation Tests

    [Test]
    public async Task Validate_WithValidPassword_ShouldNotHavePasswordError()
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "ValidPass1!",
            UserName = "test-user"
        };

        var result = await _validator.TestValidateAsync(command);

        Assert.That(result.Errors, Has.None.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Password)));
    }

    [Test]
    public async Task Validate_WithEmptyPassword_ShouldHavePasswordError()
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "",
            UserName = "test-user"
        };

        var result = await _validator.TestValidateAsync(command);

        Assert.That(result.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Password)));
    }

    [Test]
    public async Task Validate_WithPasswordTooShort_ShouldHavePasswordError()
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "Ab1!",
            UserName = "test-user"
        };

        var result = await _validator.TestValidateAsync(command);

        Assert.That(result.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Password)));
    }

    [Test]
    public async Task Validate_WithPasswordNoUppercase_ShouldHavePasswordError()
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "validpass1!",
            UserName = "test-user"
        };

        var result = await _validator.TestValidateAsync(command);

        Assert.That(result.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Password)));
    }

    [Test]
    public async Task Validate_WithPasswordNoLowercase_ShouldHavePasswordError()
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "VALIDPASS1!",
            UserName = "test-user"
        };

        var result = await _validator.TestValidateAsync(command);

        Assert.That(result.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Password)));
    }

    [Test]
    public async Task Validate_WithPasswordNoSpecialCharacter_ShouldHavePasswordError()
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "ValidPass1",
            UserName = "test-user"
        };

        var result = await _validator.TestValidateAsync(command);

        Assert.That(result.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Password)));
    }

    [TestCase("Password1!")]
    [TestCase("MyP@ssw0rd")]
    [TestCase("Str0ng!Pass")]
    [TestCase("Complex#123")]
    public async Task Validate_WithVariousValidPasswords_ShouldNotHavePasswordError(string validPassword)
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = validPassword,
            UserName = "test-user"
        };

        var result = await _validator.TestValidateAsync(command);

        Assert.That(result.Errors, Has.None.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Password)));
    }

    #endregion

    #region Full Command Validation Tests

    [Test]
    public async Task Validate_WithValidCommand_ShouldBeValid()
    {
        var command = new CreateUserCommand
        {
            Email = "user@example.com",
            Password = "SecurePass1!",
            UserName = "test-user"
        };

        var result = await _validator.TestValidateAsync(command);

        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public async Task Validate_WithAllFieldsInvalid_ShouldHaveMultipleErrors()
    {
        var command = new CreateUserCommand
        {
            Email = "",
            Password = "",
            UserName = "test-user"
        };

        var result = await _validator.TestValidateAsync(command);

        Assert.That(result.Errors, Has.Count.GreaterThanOrEqualTo(2));
        Assert.That(result.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Email)));
        Assert.That(result.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
            f => f.PropertyName == nameof(command.Password)));
    }

    #endregion
}
