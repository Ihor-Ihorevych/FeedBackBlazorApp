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
            .ReturnsAsync((Result.Success(), expectedToken));

        // Act
        var (result, token) = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(token, Is.Not.Null);
        Assert.That(token.AccessToken, Is.EqualTo("access-token"));
        Assert.That(token.RefreshToken, Is.EqualTo("refresh-token"));
        Assert.That(token.TokenType, Is.EqualTo("Bearer"));
        Assert.That(token.ExpiresIn, Is.EqualTo(3600));
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
            .ReturnsAsync((Result.Failure(["Invalid email or password."]), (AccessTokenResponse?)null));

        // Act
        var (result, token) = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Errors, Does.Contain("Invalid email or password."));
        Assert.That(token, Is.Null);
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
            .ReturnsAsync((Result.Failure(["Account is locked out. Please try again later."]), (AccessTokenResponse?)null));

        // Act
        var (result, token) = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Errors, Does.Contain("Account is locked out. Please try again later."));
        Assert.That(token, Is.Null);
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
            .ReturnsAsync((Result.Success(), new AccessTokenResponse("Bearer", "token", 3600, "refresh")));

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
            .ReturnsAsync((Result.Failure(["Invalid email or password."]), (AccessTokenResponse?)null));

        // Act
        var (result, token) = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(token, Is.Null);
    }
}
