using FB_App.Application.Common.Interfaces;

namespace FB_App.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private readonly IIdentityService _identityService;
    public CreateUserCommandValidator(IIdentityService identityService)
    {
        _identityService = identityService;
        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(v => v.Password)
            .NotEmpty()
            .Must(BeAValidPassword);


        RuleFor(v => v.UserName).NotEmpty()
            .WithMessage("Username is required.")
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters long.")
            .MaximumLength(50)
            .WithMessage("Username must not exceed 50 characters.")
            .MustAsync(async (username, cancellation) =>
            {
                var existingUser = await _identityService.GetUserNameAsync(username);
                return existingUser == null;
            })
            .WithMessage("Username is already taken.");
    }



    private static bool BeAValidPassword(string password)
    {
        return password is { Length: >= 8 } &&
               password.Any(char.IsUpper) &&
               password.Any(char.IsLower) &&
               password.Any(ch => !char.IsLetterOrDigit(ch));
    }
}
