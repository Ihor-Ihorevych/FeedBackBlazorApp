using FB_App.Application.Common.Models;
using FB_App.Domain.Constants;

namespace FB_App.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password, string role = nameof(Roles.User));

    Task<(Result Result, AccessTokenResponse? Token)> LoginUserAsync(string email, string password);

    Task<(Result Result, AccessTokenResponse? Token)> RefreshTokenAsync(string refreshToken);

    Task<Result> DeleteUserAsync(string userId);
}
