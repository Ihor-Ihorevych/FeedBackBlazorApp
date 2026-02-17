using Blazored.LocalStorage;
using FBUI.ApiClient.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;

namespace FBUI.Services;

public sealed class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly ITokenStorageService _tokenStorage;
    private readonly ILocalStorageService _localStorage;
    private readonly IServiceScopeFactory _scopeFactory;

    private static readonly SemaphoreSlim RefreshLock = new(1, 1);
    private const string AccessTokenKey_ = "accessToken";
    private const string RefreshTokenKey_ = "refreshToken";
    private const string BearerPrefix_ = "Bearer";

    public AuthorizationMessageHandler(
        ITokenStorageService tokenStorage,
        ILocalStorageService localStorage,
        IServiceScopeFactory factory)
    {
        _tokenStorage = tokenStorage;
        _localStorage = localStorage;
        _scopeFactory = factory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = _tokenStorage.AccessToken;
        if (!string.IsNullOrEmpty(token) && IsTokenExpiringSoon(token))
        {
            token = await TryRefreshTokenAsync(cancellationToken);
        }

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(BearerPrefix_, token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized && !string.IsNullOrEmpty(_tokenStorage.RefreshToken))
        {
            var newToken = await TryRefreshTokenAsync(cancellationToken);

            if (!string.IsNullOrEmpty(newToken))
            {
                var retryRequest = await CloneRequestAsync(request);
                retryRequest.Headers.Authorization = new AuthenticationHeaderValue(BearerPrefix_, newToken);

                response.Dispose();
                response = await base.SendAsync(retryRequest, cancellationToken);
            }
        }

        return response;
    }

    private static bool IsTokenExpiringSoon(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

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
            AccessTokenResponse? response = null;
            using (var scope = _scopeFactory.CreateScope())
            {
                var apiClient = scope.ServiceProvider.GetRequiredService<IFBApiClient>();
                var command = new RefreshTokenCommand { RefreshToken = refreshToken };
                response = await apiClient.PostApiUsersRefreshAsync(command, cancellationToken);
            }

            if (response?.AccessToken is not null)
            {
                await _localStorage.SetItemAsync(AccessTokenKey_, response.AccessToken, cancellationToken);
                await _localStorage.SetItemAsync(RefreshTokenKey_, response.RefreshToken, cancellationToken);
                _tokenStorage.SetTokens(response.AccessToken, response.RefreshToken);

                return response.AccessToken;
            }
        }
        catch
        {
            await _localStorage.RemoveItemAsync(AccessTokenKey_, cancellationToken);
            await _localStorage.RemoveItemAsync(RefreshTokenKey_, cancellationToken);
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
