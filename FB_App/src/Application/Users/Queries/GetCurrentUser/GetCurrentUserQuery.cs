using Ardalis.Result;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Security;

namespace FB_App.Application.Users.Queries.GetCurrentUser;

[Authorize]
public sealed record GetCurrentUserQuery : IRequest<Result<CurrentUserDto>>;


public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserDto>>
{
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public GetCurrentUserQueryHandler(IUser user, IIdentityService identityService)
    {
        _user = user;
        _identityService = identityService;
    }

    public async Task<Result<CurrentUserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_user.Id))
        {
            return Result<CurrentUserDto>.Unauthorized();
        }

        var userName = await _identityService.GetUserNameAsync(_user.Id) ?? string.Empty;
        var email = await _identityService.GetUserEmailAsync(_user.Id) ?? string.Empty;
        var roles = _user.Roles ?? [];

        var response = new CurrentUserDto(_user.Id, userName, email, roles);
        return Result<CurrentUserDto>.Success(response);
    }
}
