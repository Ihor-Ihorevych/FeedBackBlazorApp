using Blazored.LocalStorage;
using FBUI.ApiClient;
using Microsoft.AspNetCore.Components.Authorization;

namespace FBUI.Services;


public class AuthenticationService
{
    private readonly IFBApiClient _apiClient;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly TokenStorageService _tokenStorage;

    private const string AccessTokenKey = "accessToken";
    private const string RefreshTokenKey = "refreshToken";

    public AuthenticationService(
        IFBApiClient apiClient,
        ILocalStorageService localStorage,
        AuthenticationStateProvider authStateProvider,
        TokenStorageService tokenStorage)
    {
        _apiClient = apiClient;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
        _tokenStorage = tokenStorage;
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(string userName, string email, string password)
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
            return (true, null);
        }
        catch (ApiException<Exception> ex)
        {
            var errors = ex.Result.Message;
            return (false, string.Join(", ", errors));
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> LoginAsync(string email, string password)
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
                return (true, null);
            }

            return (false, "Login failed");
        }
        catch (ApiException ex) when (ex.StatusCode == 401)
        {
            return (false, "Invalid email or password");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
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

    public async Task<bool> TryRefreshTokenAsync()
    {
        try
        {
            var refreshToken = await _localStorage.GetItemAsync<string>(RefreshTokenKey);
            if (string.IsNullOrEmpty(refreshToken))
                return false;

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
                return true;
            }

            return false;
        }
        catch
        {
            await LogoutAsync();
            return false;
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
