using Ardalis.Result;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Models;
using FB_App.Application.Users.Commands.RefreshToken;
using Moq;
using NUnit.Framework;

namespace FB_App.Application.UnitTests.Users.Commands.RefreshToken;

[TestFixture]
public sealed class RefreshTokenCommandHandlerTests
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
            .ReturnsAsync(Result<AccessTokenResponse>.Success(expectedToken));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Value.AccessToken, Is.EqualTo("new-access-token"));
            Assert.That(result.Value.RefreshToken, Is.EqualTo("new-refresh-token"));
            Assert.That(result.Value.TokenType, Is.EqualTo("Bearer"));
            Assert.That(result.Value.ExpiresIn, Is.EqualTo(3600));
        }
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
            .ReturnsAsync(Result<AccessTokenResponse>.Error("Invalid or expired refresh token."));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors.Select(e => e.ToString()), Does.Contain("Invalid or expired refresh token."));
        }
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
            .ReturnsAsync(Result<AccessTokenResponse>.Error("Invalid or expired refresh token."));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
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
            .ReturnsAsync(Result<AccessTokenResponse>.Success(new AccessTokenResponse("Bearer", "token", 3600, "refresh")));

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
            .ReturnsAsync(Result<AccessTokenResponse>.Success(newToken));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Value.AccessToken, Is.EqualTo("completely-new-access-token"));
            Assert.That(result.Value.ExpiresIn, Is.EqualTo(7200));
        }
    }
}
