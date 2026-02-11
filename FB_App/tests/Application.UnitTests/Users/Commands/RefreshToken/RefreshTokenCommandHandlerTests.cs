using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Models;
using FB_App.Application.Users.Commands.RefreshToken;
using Moq;
using NUnit.Framework;

namespace FB_App.Application.UnitTests.Users.Commands.RefreshToken;

[TestFixture]
public class RefreshTokenCommandHandlerTests
{
    private Mock<IIdentityService> _identityServiceMock = null!;
    private RefreshTokenCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _handler = new RefreshTokenCommandHandler(_identityServiceMock.Object);
    }

    [Test]
    public async Task Handle_WithValidRefreshToken_ShouldReturnNewAccessToken()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshToken = "valid-refresh-token"
        };

        var expectedToken = new AccessTokenResponse(
            TokenType: "Bearer",
            AccessToken: "new-access-token",
            ExpiresIn: 3600,
            RefreshToken: "new-refresh-token");

        _identityServiceMock
            .Setup(x => x.RefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync((Result.Success(), expectedToken));

        // Act
        var (result, token) = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(token, Is.Not.Null);
        Assert.That(token.AccessToken, Is.EqualTo("new-access-token"));
        Assert.That(token.RefreshToken, Is.EqualTo("new-refresh-token"));
        Assert.That(token.TokenType, Is.EqualTo("Bearer"));
        Assert.That(token.ExpiresIn, Is.EqualTo(3600));
    }

    [Test]
    public async Task Handle_WithInvalidRefreshToken_ShouldReturnFailure()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshToken = "invalid-refresh-token"
        };

        _identityServiceMock
            .Setup(x => x.RefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync((Result.Failure(["Invalid or expired refresh token."]), (AccessTokenResponse?)null));

        // Act
        var (result, token) = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Errors, Does.Contain("Invalid or expired refresh token."));
        Assert.That(token, Is.Null);
    }

    [Test]
    public async Task Handle_WithExpiredRefreshToken_ShouldReturnFailure()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshToken = "expired-refresh-token"
        };

        _identityServiceMock
            .Setup(x => x.RefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync((Result.Failure(["Invalid or expired refresh token."]), (AccessTokenResponse?)null));

        // Act
        var (result, token) = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(token, Is.Null);
    }

    [Test]
    public async Task Handle_ShouldCallIdentityServiceWithCorrectRefreshToken()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshToken = "specific-refresh-token"
        };

        _identityServiceMock
            .Setup(x => x.RefreshTokenAsync(It.IsAny<string>()))
            .ReturnsAsync((Result.Success(), new AccessTokenResponse("Bearer", "token", 3600, "refresh")));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _identityServiceMock.Verify(x => x.RefreshTokenAsync("specific-refresh-token"), Times.Once);
    }

    [Test]
    public async Task Handle_WithValidToken_ShouldReturnDifferentAccessToken()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshToken = "valid-refresh-token"
        };

        var newToken = new AccessTokenResponse(
            TokenType: "Bearer",
            AccessToken: "completely-new-access-token",
            ExpiresIn: 7200,
            RefreshToken: "completely-new-refresh-token");

        _identityServiceMock
            .Setup(x => x.RefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync((Result.Success(), newToken));

        // Act
        var (result, token) = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(token, Is.Not.Null);
        Assert.That(token.AccessToken, Is.EqualTo("completely-new-access-token"));
        Assert.That(token.ExpiresIn, Is.EqualTo(7200));
    }
}
