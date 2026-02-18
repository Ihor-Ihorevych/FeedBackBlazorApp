using Ardalis.Result;
using FB_App.Application.Common.Interfaces;
using FB_App.Domain.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using AppAccessTokenResponse = FB_App.Application.Common.Models.AccessTokenResponse;

namespace FB_App.Infrastructure.Identity;

public class IdentityService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
    IAuthorizationService authorizationService,
    TimeProvider timeProvider,
    IOptionsMonitor<BearerTokenOptions> bearerTokenOptions) : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly IOptionsMonitor<BearerTokenOptions> _bearerTokenOptions = bearerTokenOptions;

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<string?> GetUserEmailAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.Email;
    }

    public async Task<Result<string>> CreateUserAsync(string email,
                                                      string password,
                                                      string role = nameof(Roles.User),
                                                      string userName = "")
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            return Result<string>.Error(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        await _userManager.AddToRoleAsync(user, role);

        return Result<string>.Success(user.Id);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.Succeeded
            ? Result.Success()
            : Result.Error(new ErrorList(result.Errors.Select(e => e.Description).ToArray()));
    }

    public async Task<Result<AppAccessTokenResponse>> LoginUserAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return Result<AppAccessTokenResponse>.Error("Invalid email or password.");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            if (signInResult.IsLockedOut)
            {
                return Result<AppAccessTokenResponse>.Error("Account is locked out. Please try again later.");
            }

            return Result<AppAccessTokenResponse>.Error("Invalid email or password.");
        }

        var token = await GenerateTokenAsync(user);

        return Result<AppAccessTokenResponse>.Success(token);
    }

    public async Task<Result<AppAccessTokenResponse>> RefreshTokenAsync(string refreshToken)
    {
        var options = _bearerTokenOptions.Get(IdentityConstants.BearerScheme);
        var refreshTokenProtector = options.RefreshTokenProtector;
        var refreshTicket = refreshTokenProtector.Unprotect(refreshToken);

        if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
            _timeProvider.GetUtcNow() >= expiresUtc ||
            await _signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not { } user)
        {
            return Result<AppAccessTokenResponse>.Error("Invalid or expired refresh token.");
        }

        var token = await GenerateTokenAsync(user);

        return Result<AppAccessTokenResponse>.Success(token);
    }

    private async Task<AppAccessTokenResponse> GenerateTokenAsync(ApplicationUser user)
    {
        var options = _bearerTokenOptions.Get(IdentityConstants.BearerScheme);
        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var utcNow = _timeProvider.GetUtcNow();
        var accessTokenExpiration = utcNow + options.BearerTokenExpiration;
        var refreshTokenExpiration = utcNow + options.RefreshTokenExpiration;

        var accessToken = options.BearerTokenProtector.Protect(
            CreateAuthenticationTicket(principal, accessTokenExpiration, IdentityConstants.BearerScheme));

        var refreshToken = options.RefreshTokenProtector.Protect(
            CreateAuthenticationTicket(principal, refreshTokenExpiration, IdentityConstants.BearerScheme));

        return new AppAccessTokenResponse(
            TokenType: "Bearer",
            AccessToken: accessToken,
            ExpiresIn: (long)options.BearerTokenExpiration.TotalSeconds,
            RefreshToken: refreshToken);
    }

    private static AuthenticationTicket CreateAuthenticationTicket(
        System.Security.Claims.ClaimsPrincipal principal,
        DateTimeOffset expires,
        string scheme)
    {
        var properties = new AuthenticationProperties
        {
            ExpiresUtc = expires
        };

        return new AuthenticationTicket(principal, properties, scheme);
    }
}
