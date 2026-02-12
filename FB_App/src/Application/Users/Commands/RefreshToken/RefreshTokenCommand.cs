using Ardalis.Result;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;

namespace FB_App.Application.Users.Commands.RefreshToken;

public record RefreshTokenCommand : IRequest<Result<AccessTokenResponse>>
{
    public string RefreshToken { get; init; } = string.Empty;
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AccessTokenResponse>>
{
    private readonly IIdentityService _identityService;

    public RefreshTokenCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<AccessTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.RefreshTokenAsync(request.RefreshToken);
    }
}
