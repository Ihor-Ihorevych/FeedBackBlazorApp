using Ardalis.Result;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Models;

namespace FB_App.Application.Users.Commands.LoginUser;

public sealed record LoginUserCommand : IRequest<Result<AccessTokenResponse>>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public sealed class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<AccessTokenResponse>>
{
    private readonly IIdentityService _identityService;

    public LoginUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<AccessTokenResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.LoginUserAsync(request.Email, request.Password);
    }
}
