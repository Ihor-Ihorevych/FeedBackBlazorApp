using Ardalis.Result;
using FB_App.Application.Common.Interfaces;
using FB_App.Domain.Constants;

namespace FB_App.Application.Users.Commands.CreateUser;

public record CreateUserCommand : IRequest<Result<string>>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<string>>
{
    private readonly IIdentityService _identityService;

    public CreateUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<string>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.CreateUserAsync(
            request.Email,
            request.Password,
            "User");
    }
}
