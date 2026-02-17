namespace FB_App.Application.Users.Queries.GetCurrentUser;

public sealed record CurrentUserDto(
    string UserId,
    string UserName,
    string Email,
    IReadOnlyCollection<string> Roles);
