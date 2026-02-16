using Blazored.LocalStorage;
using FBUI.ApiClient;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FBUI.Services;

public interface IAuthStateProvider
{
    Task<AuthenticationState> GetAuthenticationStateAsync();
    void NotifyUserAuthentication();
    void NotifyUserLogout();
}

public class AuthStateProvider : AuthenticationStateProvider, IAuthStateProvider
{
    private const string AccessTokenKey_ = "accessToken";
    private const string RefreshTokenKey_ = "refreshToken";
    private readonly ILocalStorageService _localStorage;
    private readonly ITokenStorageService _tokenStorage;
    private readonly IFBApiClient _apiClient;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public AuthStateProvider(ILocalStorageService localStorage, ITokenStorageService tokenStorage, IFBApiClient apiClient)
    {
        _localStorage = localStorage;
        _tokenStorage = tokenStorage;
        _apiClient = apiClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>(AccessTokenKey_);
            if (string.IsNullOrEmpty(_tokenStorage.AccessToken))
            {
                var refreshToken = await _localStorage.GetItemAsync<string>(RefreshTokenKey_);
                _tokenStorage.SetTokens(token, refreshToken);
            }

            if (string.IsNullOrEmpty(token))
                return new AuthenticationState(_anonymous);

            var userInfo = await _apiClient.GetApiUsersMeAsync();
            if (userInfo is null)
            {
                await ClearTokensAsync();
                return new AuthenticationState(_anonymous);
            }

            var claims = new List<Claim>();
            if (!string.IsNullOrWhiteSpace(userInfo.UserId))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userInfo.UserId));
            }

            if (!string.IsNullOrWhiteSpace(userInfo.UserName))
            {
                claims.Add(new Claim(ClaimTypes.Name, userInfo.UserName));
            }

            if (!string.IsNullOrWhiteSpace(userInfo.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, userInfo.Email));
            }
            var roles = userInfo?.Roles
                    ?.Where(r => !string.IsNullOrEmpty(r))
                    ?.Select(r => new Claim(ClaimTypes.Role, r)) ?? [];

            claims.AddRange(roles);


            var identity = new ClaimsIdentity(claims, "api");
            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }
        catch (ApiException ex) when (ex.StatusCode == 401)
        {
            await ClearTokensAsync();
            return new AuthenticationState(_anonymous);
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    private async Task ClearTokensAsync()
    {
        await _localStorage.RemoveItemAsync(AccessTokenKey_);
        await _localStorage.RemoveItemAsync(RefreshTokenKey_);
        _tokenStorage.ClearTokens();
    }

    public void NotifyUserAuthentication()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void NotifyUserLogout()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }
}
