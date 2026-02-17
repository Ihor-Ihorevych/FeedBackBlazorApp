namespace FB_App.Application.Common.Models;

public sealed record AccessTokenResponse(
    string TokenType,
    string AccessToken,
    long ExpiresIn,
    string RefreshToken);
