using System.Security.Claims;
using Blazored.LocalStorage;
using FBUI.ApiClient;
using Microsoft.AspNetCore.Components.Authorization;

namespace FBUI.Services;

public class AuthStateProvider : AuthenticationStateProvider
{
    private const string AccessTokenKey_ = "accessToken";
    private const string RefreshTokenKey_ = "refreshToken";
    private readonly ILocalStorageService _localStorage;
    private readonly ITokenStorageService _tokenStorage;
    private readonly IFBApiClient _apiClient;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
    private bool _initialized;

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
            var token = await _localStorage.GetItemAsync<string>("accessToken");
            if (!_initialized)
            {
                var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");
                _tokenStorage.SetTokens(token, refreshToken);
                _initialized = true;
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

            if (userInfo.Roles is not null)
            {
                foreach (var role in userInfo.Roles.Where(r => !string.IsNullOrEmpty(r)))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

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
        _initialized = false;
    }

    public void NotifyUserAuthentication(string token)
    {
        _ = token;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void NotifyUserLogout()
    {
        _initialized = false;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }
}
