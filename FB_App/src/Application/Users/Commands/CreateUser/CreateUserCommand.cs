using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Models;
using FB_App.Domain.Constants;

namespace FB_App.Application.Users.Commands.CreateUser;

public record CreateUserCommand : IRequest<(Result Result, string UserId)>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, (Result Result, string UserId)>
{
    private readonly IIdentityService _identityService;

    public CreateUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<(Result Result, string UserId)> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.CreateUserAsync(
            request.Email,
            request.Password,
            "User");
    }
}
