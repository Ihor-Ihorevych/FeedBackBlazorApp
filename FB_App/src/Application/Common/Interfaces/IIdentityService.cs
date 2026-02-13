using Ardalis.Result;
using FB_App.Application.Common.Models;
using FB_App.Domain.Constants;

namespace FB_App.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<string?> GetUserEmailAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<Result<string>> CreateUserAsync(string email, string password, string role = nameof(Roles.User), string userName = "");

    Task<Result<AccessTokenResponse>> LoginUserAsync(string email, string password);

    Task<Result<AccessTokenResponse>> RefreshTokenAsync(string refreshToken);

    Task<Result> DeleteUserAsync(string userId);
}
