using Ardalis.Result;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Models;
using FB_App.Application.Users.Commands.LoginUser;
using Moq;
using NUnit.Framework;

namespace FB_App.Application.UnitTests.Users.Commands.LoginUser;

[TestFixture]
public class LoginUserCommandHandlerTests
{
    private Mock<IIdentityService> _identityServiceMock = null!;
    private LoginUserCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _handler = new LoginUserCommandHandler(_identityServiceMock.Object);
    }

    [Test]
    public async Task Handle_WithValidCredentials_ShouldReturnSuccessWithToken()
    {
        // Arrange
        var command = new LoginUserCommand
        {
            Email = "test@example.com",
            Password = "ValidPass1!"
        };

        var expectedToken = new AccessTokenResponse(
            TokenType: "Bearer",
            AccessToken: "access-token",
            ExpiresIn: 3600,
            RefreshToken: "refresh-token");

        _identityServiceMock
            .Setup(x => x.LoginUserAsync(command.Email, command.Password))
            .ReturnsAsync(Result<AccessTokenResponse>.Success(expectedToken));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.AccessToken, Is.EqualTo("access-token"));
        Assert.That(result.Value.RefreshToken, Is.EqualTo("refresh-token"));
        Assert.That(result.Value.TokenType, Is.EqualTo("Bearer"));
        Assert.That(result.Value.ExpiresIn, Is.EqualTo(3600));
    }

    [Test]
    public async Task Handle_WithInvalidCredentials_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginUserCommand
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        _identityServiceMock
            .Setup(x => x.LoginUserAsync(command.Email, command.Password))
            .ReturnsAsync(Result<AccessTokenResponse>.Error("Invalid email or password."));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors.Select(e => e.ToString()), Does.Contain("Invalid email or password."));
    }

    [Test]
    public async Task Handle_WithLockedOutAccount_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginUserCommand
        {
            Email = "locked@example.com",
            Password = "ValidPass1!"
        };

        _identityServiceMock
            .Setup(x => x.LoginUserAsync(command.Email, command.Password))
            .ReturnsAsync(Result<AccessTokenResponse>.Error("Account is locked out. Please try again later."));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors.Select(e => e.ToString()), Does.Contain("Account is locked out. Please try again later."));
    }

    [Test]
    public async Task Handle_ShouldCallIdentityServiceWithCorrectParameters()
    {
        // Arrange
        var command = new LoginUserCommand
        {
            Email = "user@example.com",
            Password = "Password123!"
        };

        _identityServiceMock
            .Setup(x => x.LoginUserAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result<AccessTokenResponse>.Success(new AccessTokenResponse("Bearer", "token", 3600, "refresh")));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _identityServiceMock.Verify(x => x.LoginUserAsync(
            command.Email,
            command.Password), Times.Once);
    }

    [Test]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginUserCommand
        {
            Email = "nonexistent@example.com",
            Password = "AnyPassword1!"
        };

        _identityServiceMock
            .Setup(x => x.LoginUserAsync(command.Email, command.Password))
            .ReturnsAsync(Result<AccessTokenResponse>.Error("Invalid email or password."));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
    }
}
