using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Models;
using FB_App.Application.Users.Commands.RefreshToken;
using Moq;
using NUnit.Framework;
using Shouldly;

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
        result.Succeeded.ShouldBeTrue();
        token.ShouldNotBeNull();
        token.AccessToken.ShouldBe("new-access-token");
        token.RefreshToken.ShouldBe("new-refresh-token");
        token.TokenType.ShouldBe("Bearer");
        token.ExpiresIn.ShouldBe(3600);
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
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain("Invalid or expired refresh token.");
        token.ShouldBeNull();
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
        result.Succeeded.ShouldBeFalse();
        token.ShouldBeNull();
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
        result.Succeeded.ShouldBeTrue();
        token.ShouldNotBeNull();
        token.AccessToken.ShouldBe("completely-new-access-token");
        token.ExpiresIn.ShouldBe(7200);
    }
}
