namespace FBUI.Services;

public interface ITokenStorageService
{
    string? AccessToken { get; }
    string? RefreshToken { get; }

    void ClearTokens();
    void SetTokens(string? accessToken, string? refreshToken);
}

public sealed class TokenStorageService : ITokenStorageService
{
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public void SetTokens(string? accessToken, string? refreshToken) => (AccessToken, RefreshToken) = (accessToken, refreshToken);
    public void ClearTokens() => SetTokens(null, null);
}
