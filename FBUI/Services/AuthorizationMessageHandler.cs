using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using Blazored.LocalStorage;
using FBUI.ApiClient;
using FBUI.Configuration;
using Microsoft.Extensions.Options;

namespace FBUI.Services;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly ITokenStorageService _tokenStorage;
    private readonly ILocalStorageService _localStorage;
    private readonly IOptions<ApiSettings> _apiSettings;
    private readonly IServiceProvider _serviceProvider;

    private static readonly SemaphoreSlim RefreshLock = new(1, 1);
    private const string AccessTokenKey = "accessToken";
    private const string RefreshTokenKey = "refreshToken";

    public AuthorizationMessageHandler(
        ITokenStorageService tokenStorage,
        ILocalStorageService localStorage,
        IOptions<ApiSettings> apiSettings,
        IServiceProvider serviceProvider)
    {
        _tokenStorage = tokenStorage;
        _localStorage = localStorage;
        _apiSettings = apiSettings;
        _serviceProvider = serviceProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        var token = _tokenStorage.AccessToken;

        // Check if token is about to expire (within 30 seconds)
        if (!string.IsNullOrEmpty(token) && IsTokenExpiringSoon(token))
        {
            token = await TryRefreshTokenAsync(cancellationToken);
        }

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // If we get 401, try to refresh token and retry the request
        if (response.StatusCode == HttpStatusCode.Unauthorized && !string.IsNullOrEmpty(_tokenStorage.RefreshToken))
        {
            var newToken = await TryRefreshTokenAsync(cancellationToken);

            if (!string.IsNullOrEmpty(newToken))
            {
                // Clone the request and retry with new token
                var retryRequest = await CloneRequestAsync(request);
                retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);

                response.Dispose();
                response = await base.SendAsync(retryRequest, cancellationToken);
            }
        }

        return response;
    }

    private bool IsTokenExpiringSoon(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Token is expiring if less than 30 seconds remaining
            return jwtToken.ValidTo <= DateTime.UtcNow.AddSeconds(30);
        }
        catch
        {
            return false;
        }
    }

    private async Task<string?> TryRefreshTokenAsync(CancellationToken cancellationToken)
    {
        await RefreshLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check if another thread already refreshed
            var currentToken = _tokenStorage.AccessToken;
            if (!string.IsNullOrEmpty(currentToken) && !IsTokenExpiringSoon(currentToken))
            {
                return currentToken;
            }

            var refreshToken = _tokenStorage.RefreshToken;
            if (string.IsNullOrEmpty(refreshToken))
            {
                return null;
            }

            // Create a new HttpClient without the handler to avoid recursion
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri(_apiSettings.Value.BaseAddress)
            };

            var apiClient = new FBApiClient(_apiSettings.Value.BaseAddress, httpClient);

            var command = new RefreshTokenCommand { RefreshToken = refreshToken };
            var response = await apiClient.PostApiUsersRefreshAsync(command, cancellationToken);

            if (response?.AccessToken is not null)
            {
                await _localStorage.SetItemAsync(AccessTokenKey, response.AccessToken, cancellationToken);
                await _localStorage.SetItemAsync(RefreshTokenKey, response.RefreshToken, cancellationToken);
                _tokenStorage.SetTokens(response.AccessToken, response.RefreshToken);

                return response.AccessToken;
            }
        }
        catch
        {
            // Refresh failed - clear tokens
            await _localStorage.RemoveItemAsync(AccessTokenKey, cancellationToken);
            await _localStorage.RemoveItemAsync(RefreshTokenKey, cancellationToken);
            _tokenStorage.ClearTokens();
        }
        finally
        {
            RefreshLock.Release();
        }

        return null;
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version
        };

        if (request.Content != null)
        {
            var content = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(content);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var option in request.Options)
        {
            clone.Options.TryAdd(option.Key, option.Value);
        }

        return clone;
    }
}
