using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Models;

namespace FB_App.Application.Users.Commands.LoginUser;

public record LoginUserCommand : IRequest<(Result Result, AccessTokenResponse? Token)>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, (Result Result, AccessTokenResponse? Token)>
{
    private readonly IIdentityService _identityService;

    public LoginUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<(Result Result, AccessTokenResponse? Token)> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.LoginUserAsync(request.Email, request.Password);
    }
}
