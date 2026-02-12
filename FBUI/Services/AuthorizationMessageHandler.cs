namespace FBUI.Services;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly TokenStorageService _tokenStorage;

    public AuthorizationMessageHandler(TokenStorageService tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        var token = _tokenStorage.AccessToken;

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
