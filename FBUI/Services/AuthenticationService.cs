using Ardalis.Result;
using Blazored.LocalStorage;
using FBUI.ApiClient;
using Microsoft.AspNetCore.Components.Authorization;

namespace FBUI.Services;

public interface IAuthenticationService
{
    Task<string?> GetAccessTokenAsync();
    Task InitializeAsync();
    Task<Result<AccessTokenResponse>> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<Result> RegisterAsync(string userName, string email, string password);
    Task<Result> TryRefreshTokenAsync();
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IFBApiClient _apiClient;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ITokenStorageService _tokenStorage;

    private const string AccessTokenKey = "accessToken";
    private const string RefreshTokenKey = "refreshToken";

    public AuthenticationService(
        IFBApiClient apiClient,
        ILocalStorageService localStorage,
        AuthenticationStateProvider authStateProvider,
        ITokenStorageService tokenStorage)
    {
        _apiClient = apiClient;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
        _tokenStorage = tokenStorage;
    }

    public async Task<Result> RegisterAsync(string userName, string email, string password)
    {
        try
        {
            var command = new CreateUserCommand
            {
                Email = email,
                Password = password,
                UserName = userName
            };

            await _apiClient.PostApiUsersRegisterAsync(command);
            return Result.Success();
        }
        catch (ApiException<Exception> ex)
        {
            return Result.Error(ex.Result.Message ?? "Registration failed");
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }

    public async Task<Result<AccessTokenResponse>> LoginAsync(string email, string password)
    {
        try
        {
            var command = new LoginUserCommand
            {
                Email = email,
                Password = password
            };

            var response = await _apiClient.PostApiUsersLoginAsync(command);

            if (response?.AccessToken is not null)
            {
                await _localStorage.SetItemAsync(AccessTokenKey, response.AccessToken);
                await _localStorage.SetItemAsync(RefreshTokenKey, response.RefreshToken);
                _tokenStorage.SetTokens(response.AccessToken, response.RefreshToken);

                ((AuthStateProvider)_authStateProvider).NotifyUserAuthentication(response.AccessToken);
                return Result<AccessTokenResponse>.Success(response);
            }

            return Result<AccessTokenResponse>.Error("Login failed");
        }
        catch (ApiException ex) when (ex.StatusCode == 401)
        {
            return Result<AccessTokenResponse>.Unauthorized();
        }
        catch (Exception ex)
        {
            return Result<AccessTokenResponse>.Error(ex.Message);
        }
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync(AccessTokenKey);
        await _localStorage.RemoveItemAsync(RefreshTokenKey);
        _tokenStorage.ClearTokens();
        ((AuthStateProvider)_authStateProvider).NotifyUserLogout();
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>(AccessTokenKey);
    }

    public async Task<Result> TryRefreshTokenAsync()
    {
        try
        {
            var refreshToken = await _localStorage.GetItemAsync<string>(RefreshTokenKey);
            if (string.IsNullOrEmpty(refreshToken))
                return Result.Error("No refresh token available");

            var command = new RefreshTokenCommand
            {
                RefreshToken = refreshToken
            };

            var response = await _apiClient.PostApiUsersRefreshAsync(command);

            if (response?.AccessToken is not null)
            {
                await _localStorage.SetItemAsync(AccessTokenKey, response.AccessToken);
                await _localStorage.SetItemAsync(RefreshTokenKey, response.RefreshToken);
                _tokenStorage.SetTokens(response.AccessToken, response.RefreshToken);

                ((AuthStateProvider)_authStateProvider).NotifyUserAuthentication(response.AccessToken);
                return Result.Success();
            }

            return Result.Error("Token refresh failed");
        }
        catch
        {
            await LogoutAsync();
            return Result.Error("Token refresh failed");
        }
    }

    /// <summary>
    /// Initializes in-memory token storage from local storage.
    /// Should be called during app initialization.
    /// </summary>
    public async Task InitializeAsync()
    {
        var accessToken = await _localStorage.GetItemAsync<string>(AccessTokenKey);
        var refreshToken = await _localStorage.GetItemAsync<string>(RefreshTokenKey);
        _tokenStorage.SetTokens(accessToken, refreshToken);
    }
}
