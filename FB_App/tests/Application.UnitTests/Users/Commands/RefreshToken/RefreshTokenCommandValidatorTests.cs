using FB_App.Application.Users.Commands.RefreshToken;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace FB_App.Application.UnitTests.Users.Commands.RefreshToken;

[TestFixture]
public class RefreshTokenCommandValidatorTests
{
    private RefreshTokenCommandValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new RefreshTokenCommandValidator();
    }

    [Test]
    public void Validate_WithValidRefreshToken_ShouldNotHaveError()
    {
        var command = new RefreshTokenCommand
        {
            RefreshToken = "valid-refresh-token-value"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.RefreshToken);
    }

    [Test]
    public void Validate_WithEmptyRefreshToken_ShouldHaveError()
    {
        var command = new RefreshTokenCommand
        {
            RefreshToken = ""
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.RefreshToken)
            .WithErrorMessage("Refresh token is required.");
    }

    [Test]
    public void Validate_WithWhitespaceRefreshToken_ShouldHaveError()
    {
        var command = new RefreshTokenCommand
        {
            RefreshToken = "   "
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }

    [Test]
    public void Validate_WithValidCommand_ShouldBeValid()
    {
        var command = new RefreshTokenCommand
        {
            RefreshToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
