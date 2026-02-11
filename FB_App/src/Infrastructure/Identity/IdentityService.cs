using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Models;
using FB_App.Domain.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using AppAccessTokenResponse = FB_App.Application.Common.Models.AccessTokenResponse;

namespace FB_App.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly TimeProvider _timeProvider;
    private readonly IOptionsMonitor<BearerTokenOptions> _bearerTokenOptions;


    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService,
        TimeProvider timeProvider,
        IOptionsMonitor<BearerTokenOptions> bearerTokenOptions)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _timeProvider = timeProvider;
        _bearerTokenOptions = bearerTokenOptions;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password, string role = nameof(Roles.User))
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userName,
        };

        var result = await _userManager.CreateAsync(user, password);
        await _userManager.AddToRoleAsync(user, role);

        return (result.ToApplicationResult(), user.Id);
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

        return result.ToApplicationResult();
    }

    public async Task<(Result Result, AppAccessTokenResponse? Token)> LoginUserAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return (Result.Failure(["Invalid email or password."]), null);
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            if (signInResult.IsLockedOut)
            {
                return (Result.Failure(["Account is locked out. Please try again later."]), null);
            }

            return (Result.Failure(["Invalid email or password."]), null);
        }

        var token = await GenerateTokenAsync(user);

        return (Result.Success(), token);
    }

    public async Task<(Result Result, AppAccessTokenResponse? Token)> RefreshTokenAsync(string refreshToken)
    {
        var options = _bearerTokenOptions.Get(IdentityConstants.BearerScheme);
        var refreshTokenProtector = options.RefreshTokenProtector;
        var refreshTicket = refreshTokenProtector.Unprotect(refreshToken);

        if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
            _timeProvider.GetUtcNow() >= expiresUtc ||
            await _signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not { } user)
        {
            return (Result.Failure(["Invalid or expired refresh token."]), null);
        }

        var token = await GenerateTokenAsync(user);

        return (Result.Success(), token);
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
