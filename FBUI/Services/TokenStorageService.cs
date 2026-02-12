namespace FBUI.Services;


public class TokenStorageService
{
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }

    public void SetTokens(string? accessToken, string? refreshToken) => (AccessToken, RefreshToken) = (accessToken, refreshToken);
    public void ClearTokens() => SetTokens(null, null);
}
