namespace FB_App.Application.Users.Queries.GetCurrentUser;

public record CurrentUserDto(
    string UserId,
    string UserName,
    string Email,
    IReadOnlyCollection<string> Roles);
