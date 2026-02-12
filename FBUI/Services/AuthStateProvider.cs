using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace FBUI.Services;

public class AuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly TokenStorageService _tokenStorage;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
    private bool _initialized;

    public AuthStateProvider(ILocalStorageService localStorage, TokenStorageService tokenStorage)
    {
        _localStorage = localStorage;
        _tokenStorage = tokenStorage;
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

            var claims = ParseClaimsFromJwt(token);
            var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
            if (expClaim is not null && long.TryParse(expClaim.Value, out var exp))
            {
                var expDate = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                if (expDate < DateTime.UtcNow)
                    return new AuthenticationState(_anonymous);
            }

            var identity = new ClaimsIdentity(claims, "jwt");
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
        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLogout()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }

    private static List<Claim> ParseClaimsFromJwt(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(jwt))
            return [];

        var token = handler.ReadJwtToken(jwt);
        var claims = new List<Claim>();

        foreach (var claim in token.Claims)
        {
            claims.Add(claim);

            if (claim.Type is "role" or "roles")
            {
                foreach (var role in ParseRoles(claim.Value))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }
        }

        return claims;
    }

    private static IEnumerable<string> ParseRoles(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            yield break;
        }

        var trimmed = value.Trim();
        if (trimmed.StartsWith("[", StringComparison.Ordinal))
        {
            using var doc = JsonDocument.Parse(trimmed);
            foreach (var item in doc.RootElement.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    var role = item.GetString();
                    if (!string.IsNullOrWhiteSpace(role))
                    {
                        yield return role;
                    }
                }
            }

            yield break;
        }

        foreach (var role in trimmed.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            yield return role;
        }
    }
}
