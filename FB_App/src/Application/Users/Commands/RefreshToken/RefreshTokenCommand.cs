using Ardalis.Result;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Models;

namespace FB_App.Application.Users.Commands.RefreshToken;

public sealed record RefreshTokenCommand : IRequest<Result<AccessTokenResponse>>
{
    public string RefreshToken { get; init; } = string.Empty;
}

public sealed class RefreshTokenCommandHandler(IIdentityService identityService) : IRequestHandler<RefreshTokenCommand, Result<AccessTokenResponse>>
{
    private readonly IIdentityService _identityService = identityService;

    public async Task<Result<AccessTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.RefreshTokenAsync(request.RefreshToken);
    }
}
