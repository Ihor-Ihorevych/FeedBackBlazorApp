using System.Security.Claims;
using Blazored.LocalStorage;
using FBUI.ApiClient;
using Microsoft.AspNetCore.Components.Authorization;

namespace FBUI.Services;

public class AuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly TokenStorageService _tokenStorage;
    private readonly IFBApiClient _apiClient;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
    private bool _initialized;

    public AuthStateProvider(ILocalStorageService localStorage, TokenStorageService tokenStorage, IFBApiClient apiClient)
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
                foreach (var role in userInfo.Roles)
                {
                    if (!string.IsNullOrWhiteSpace(role))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }
            }

            var identity = new ClaimsIdentity(claims, "api");
            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public void NotifyUserAuthentication(string token)
    {
        _ = token;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void NotifyUserLogout()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }
}
