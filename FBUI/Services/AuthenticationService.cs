using Ardalis.Result;
using Blazored.LocalStorage;
using FBUI.ApiClient.Contracts;
using FBUI.Extensions;

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

public sealed class AuthenticationService(
    IFBApiClient apiClient,
    ILocalStorageService localStorage,
    IAuthStateProvider authStateProvider,
    ITokenStorageService tokenStorage) : IAuthenticationService
{
    private readonly IFBApiClient _apiClient = apiClient;
    private readonly ILocalStorageService _localStorage = localStorage;
    private readonly IAuthStateProvider _authStateProvider = authStateProvider;
    private readonly ITokenStorageService _tokenStorage = tokenStorage;

    private const string AccessTokenKey = "accessToken";
    private const string RefreshTokenKey = "refreshToken";

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
        catch (ApiException<ProblemDetails> ex)
        {
            return Result.Error(ex.GetProblemDetails());
        }
        catch (ApiException ex)
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

                _authStateProvider.NotifyUserAuthentication();
                return Result<AccessTokenResponse>.Success(response);
            }

            return Result<AccessTokenResponse>.Error("Login failed");
        }
        catch (ApiException<ProblemDetails> ex) when (ex.StatusCode == 401)
        {
            return Result<AccessTokenResponse>.Unauthorized();
        }
        catch (ApiException<HttpValidationProblemDetails> ex)
        {
            return Result<AccessTokenResponse>.Invalid(validationError:

                new ValidationError
                {
                    ErrorMessage = ex.GetValidationErrors()
                }
            );
        }
        catch (ApiException ex)
        {
            return Result<AccessTokenResponse>.Error(ex.Message);
        }
    }



    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync(AccessTokenKey);
        await _localStorage.RemoveItemAsync(RefreshTokenKey);
        _tokenStorage.ClearTokens();
        _authStateProvider.NotifyUserLogout();
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
                _authStateProvider.NotifyUserAuthentication();
                return Result.Success();
            }

            return Result.Error("Token refresh failed");
        }
        catch (ApiException)
        {
            await LogoutAsync();
            return Result.Error("Token refresh failed");
        }
    }

    public async Task InitializeAsync()
    {
        var accessToken = await _localStorage.GetItemAsync<string>(AccessTokenKey);
        var refreshToken = await _localStorage.GetItemAsync<string>(RefreshTokenKey);
        _tokenStorage.SetTokens(accessToken, refreshToken);
    }
}
