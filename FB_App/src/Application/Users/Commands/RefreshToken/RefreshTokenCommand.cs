using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Models;

namespace FB_App.Application.Users.Commands.RefreshToken;

public record RefreshTokenCommand : IRequest<(Result Result, AccessTokenResponse? Token)>
{
    public string RefreshToken { get; init; } = string.Empty;
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, (Result Result, AccessTokenResponse? Token)>
{
    private readonly IIdentityService _identityService;

    public RefreshTokenCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<(Result Result, AccessTokenResponse? Token)> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.RefreshTokenAsync(request.RefreshToken);
    }
}
