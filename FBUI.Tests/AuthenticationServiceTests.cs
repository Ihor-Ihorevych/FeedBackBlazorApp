using Ardalis.Result;
using Blazored.LocalStorage;
using FBUI.ApiClient.Contracts;
using FBUI.Services;
using Moq;

namespace FBUI.Tests;

public sealed class AuthenticationServiceTests
{
    private Mock<IFBApiClient> _apiClientMock;
    private Mock<ILocalStorageService> _localStorageMock;
    private Mock<IAuthStateProvider> _authStateProviderMock;
    private Mock<ITokenStorageService> _tokenStorageMock;
    private IAuthenticationService _authService;

    [SetUp]
    public void SetUp()
    {
        _apiClientMock = new Mock<IFBApiClient>();
        _localStorageMock = new Mock<ILocalStorageService>();
        _authStateProviderMock = new Mock<IAuthStateProvider>();
        _tokenStorageMock = new Mock<ITokenStorageService>();
        _authService = new AuthenticationService(
            _apiClientMock.Object,
            _localStorageMock.Object,
            _authStateProviderMock.Object,
            _tokenStorageMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _apiClientMock.Reset();
        _localStorageMock.Reset();
        _authStateProviderMock.Reset();
        _tokenStorageMock.Reset();
    }



    [Test]
    public async Task RegisterAsync_Success_ReturnsSuccess()
    {
        _apiClientMock
            .Setup(x => x.PostApiUsersRegisterAsync(It.IsAny<CreateUserCommand>()))
            .Returns(Task.FromResult("some-id"));

        var result = await _authService.RegisterAsync("user", "email@test.com", "pass");

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task RegisterAsync_ApiException_ReturnsError()
    {
        _apiClientMock
            .Setup(x => x.PostApiUsersRegisterAsync(It.IsAny<CreateUserCommand>()))
            .ThrowsAsync(new ApiException("fail", 400, null, null, null));

        var result = await _authService.RegisterAsync("user", "email@test.com", "pass");

        Assert.That(result.IsError, Is.True);
    }

    [Test]
    public async Task LoginAsync_Success_StoresTokensAndReturnsSuccess()
    {
        var response = new AccessTokenResponse { AccessToken = "token", RefreshToken = "refresh" };

        _apiClientMock
            .Setup(x => x.PostApiUsersLoginAsync(It.IsAny<LoginUserCommand>()))
            .ReturnsAsync(response);

        var result = await _authService.LoginAsync("email@test.com", "pass");

        Assert.That(result.IsSuccess, Is.True);

        _localStorageMock.Verify(x => x.SetItemAsync("accessToken", "token", CancellationToken.None), Times.Once);
        _localStorageMock.Verify(x => x.SetItemAsync("refreshToken", "refresh", CancellationToken.None), Times.Once);
        _tokenStorageMock.Verify(x => x.SetTokens("token", "refresh"), Times.Once);
        _authStateProviderMock.Verify(x => x.NotifyUserAuthentication(), Times.Once);
    }

    [Test]
    public async Task LoginAsync_NoAccessToken_ReturnsError()
    {
        _apiClientMock
            .Setup(x => x.PostApiUsersLoginAsync(It.IsAny<LoginUserCommand>()))
            .ReturnsAsync(new AccessTokenResponse());

        var result = await _authService.LoginAsync("email@test.com", "pass");

        Assert.That(result.IsError, Is.True);
    }

    [Test]
    public async Task LoginAsync_ApiException401_ReturnsUnauthorized()
    {
        var ex = new ApiException<ProblemDetails>("unauthorized", 401, null, null, null, null);

        _apiClientMock
            .Setup(x => x.PostApiUsersLoginAsync(It.IsAny<LoginUserCommand>()))
            .ThrowsAsync(ex);

        var result = await _authService.LoginAsync("email@test.com", "pass");

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Unauthorized));
    }

    [Test]
    public async Task LogoutAsync_RemovesTokensAndNotifies()
    {
        await _authService.LogoutAsync();

        _localStorageMock.Verify(x => x.RemoveItemAsync("accessToken", CancellationToken.None), Times.Once);
        _localStorageMock.Verify(x => x.RemoveItemAsync("refreshToken", CancellationToken.None), Times.Once);
        _tokenStorageMock.Verify(x => x.ClearTokens(), Times.Once);
        _authStateProviderMock.Verify(x => x.NotifyUserLogout(), Times.Once);
    }

    [Test]
    public async Task GetAccessTokenAsync_ReturnsToken()
    {
        _localStorageMock
            .Setup(x => x.GetItemAsync<string>("accessToken", CancellationToken.None))
            .ReturnsAsync("token");

        var token = await _authService.GetAccessTokenAsync();

        Assert.That(token, Is.EqualTo("token"));
    }

    [Test]
    public async Task TryRefreshTokenAsync_NoToken_ReturnsError()
    {
        _localStorageMock
            .Setup(x => x.GetItemAsync<string>("refreshToken", CancellationToken.None))
            .ReturnsAsync((string)null);

        var result = await _authService.TryRefreshTokenAsync();

        Assert.That(result.IsError, Is.True);
    }

    [Test]
    public async Task TryRefreshTokenAsync_Success_StoresTokensAndReturnsSuccess()
    {
        _localStorageMock
            .Setup(x => x.GetItemAsync<string>("refreshToken", CancellationToken.None))
            .ReturnsAsync("refresh");

        var response = new AccessTokenResponse
        {
            AccessToken = "token",
            RefreshToken = "refresh2"
        };

        _apiClientMock
            .Setup(x => x.PostApiUsersRefreshAsync(It.IsAny<RefreshTokenCommand>()))
            .ReturnsAsync(response);

        var result = await _authService.TryRefreshTokenAsync();

        Assert.That(result.IsSuccess, Is.True);

        _localStorageMock.Verify(x => x.SetItemAsync("accessToken", "token", CancellationToken.None), Times.Once);
        _localStorageMock.Verify(x => x.SetItemAsync("refreshToken", "refresh2", CancellationToken.None), Times.Once);
        _tokenStorageMock.Verify(x => x.SetTokens("token", "refresh2"), Times.Once);
        _authStateProviderMock.Verify(x => x.NotifyUserAuthentication(), Times.Once);
    }

    [Test]
    public async Task TryRefreshTokenAsync_ApiException_LogsOutAndReturnsError()
    {
        _localStorageMock
            .Setup(x => x.GetItemAsync<string>("refreshToken", CancellationToken.None))
            .ReturnsAsync("refresh");

        _apiClientMock
            .Setup(x => x.PostApiUsersRefreshAsync(It.IsAny<RefreshTokenCommand>()))
            .ThrowsAsync(new ApiException("fail", 400, null, null, null));

        var result = await _authService.TryRefreshTokenAsync();

        Assert.That(result.IsError, Is.True);

        _localStorageMock.Verify(x => x.RemoveItemAsync("accessToken", CancellationToken.None), Times.Once);
        _localStorageMock.Verify(x => x.RemoveItemAsync("refreshToken", CancellationToken.None), Times.Once);
        _tokenStorageMock.Verify(x => x.ClearTokens(), Times.Once);
        _authStateProviderMock.Verify(x => x.NotifyUserLogout(), Times.Once);
    }

    [Test]
    public async Task InitializeAsync_SetsTokens()
    {
        _localStorageMock
            .Setup(x => x.GetItemAsync<string>("accessToken", CancellationToken.None))
            .ReturnsAsync("token");

        _localStorageMock
            .Setup(x => x.GetItemAsync<string>("refreshToken", CancellationToken.None))
            .ReturnsAsync("refresh");

        await _authService.InitializeAsync();

        _tokenStorageMock.Verify(x => x.SetTokens("token", "refresh"), Times.Once);
    }
}
