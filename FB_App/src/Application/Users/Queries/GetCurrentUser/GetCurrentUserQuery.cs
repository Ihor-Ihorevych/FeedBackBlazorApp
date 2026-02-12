using Ardalis.Result;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Models;
using FB_App.Application.Common.Security;

namespace FB_App.Application.Users.Queries.GetCurrentUser;

[Authorize]
public record GetCurrentUserQuery : IRequest<Result<CurrentUserResponse>>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserResponse>>
{
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public GetCurrentUserQueryHandler(IUser user, IIdentityService identityService)
    {
        _user = user;
        _identityService = identityService;
    }

    public async Task<Result<CurrentUserResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_user.Id))
        {
            return Result<CurrentUserResponse>.Unauthorized();
        }

        var userName = await _identityService.GetUserNameAsync(_user.Id) ?? string.Empty;
        var email = await _identityService.GetUserEmailAsync(_user.Id) ?? string.Empty;
        var roles = _user.Roles ?? [];

        var response = new CurrentUserResponse(_user.Id, userName, email, roles);
        return Result<CurrentUserResponse>.Success(response);
    }
}
