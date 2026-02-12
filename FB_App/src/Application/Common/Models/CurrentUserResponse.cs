namespace FB_App.Application.Common.Models;

public record CurrentUserResponse(
    string UserId,
    string UserName,
    string Email,
    IReadOnlyCollection<string> Roles);
