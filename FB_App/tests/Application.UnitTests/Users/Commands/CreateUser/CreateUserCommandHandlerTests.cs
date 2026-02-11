using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Models;
using FB_App.Application.Users.Commands.CreateUser;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace FB_App.Application.UnitTests.Users.Commands.CreateUser;

[TestFixture]
public class CreateUserCommandHandlerTests
{
    private Mock<IIdentityService> _identityServiceMock = null!;
    private CreateUserCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _handler = new CreateUserCommandHandler(_identityServiceMock.Object);
    }

    [Test]
    public async Task Handle_WithValidCommand_ShouldCallIdentityServiceWithCorrectParameters()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "ValidPass1!"
        };

        var expectedUserId = Guid.NewGuid().ToString();
        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Email, command.Password, "User"))
            .ReturnsAsync((Result.Success(), expectedUserId));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _identityServiceMock.Verify(x => x.CreateUserAsync(
            command.Email,
            command.Password,
            "User"), Times.Once);
    }

    [Test]
    public async Task Handle_WhenIdentityServiceSucceeds_ShouldReturnSuccessResult()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "ValidPass1!"
        };

        var expectedUserId = Guid.NewGuid().ToString();
        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Email, command.Password, "User"))
            .ReturnsAsync((Result.Success(), expectedUserId));

        // Act
        var (result, userId) = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeTrue();
        userId.ShouldBe(expectedUserId);
    }

    [Test]
    public async Task Handle_WhenIdentityServiceFails_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "ValidPass1!"
        };

        var errors = new[] { "Email already exists." };
        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Email, command.Password, "User"))
            .ReturnsAsync((Result.Failure(errors), string.Empty));

        // Act
        var (result, userId) = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain("Email already exists.");
        userId.ShouldBeEmpty();
    }

    [Test]
    public async Task Handle_ShouldAlwaysAssignUserRole()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "newuser@example.com",
            Password = "SecurePass1!"
        };

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((Result.Success(), "user-id"));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _identityServiceMock.Verify(x => x.CreateUserAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            "User"), Times.Once);
    }

    [Test]
    public async Task Handle_WithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "ValidPass1!"
        };

        using var cts = new CancellationTokenSource();
        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Email, command.Password, "User"))
            .ReturnsAsync((Result.Success(), "user-id"));

        // Act
        var (result, _) = await _handler.Handle(command, cts.Token);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }
}
