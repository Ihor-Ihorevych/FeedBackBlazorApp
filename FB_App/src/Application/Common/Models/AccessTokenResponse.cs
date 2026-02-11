namespace FB_App.Application.Common.Models;

public record AccessTokenResponse(
    string TokenType,
    string AccessToken,
    long ExpiresIn,
    string RefreshToken);
