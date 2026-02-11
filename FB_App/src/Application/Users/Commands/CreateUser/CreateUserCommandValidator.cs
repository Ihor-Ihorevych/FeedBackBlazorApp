namespace FB_App.Application.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(v => v.Password)
            .NotEmpty()
            .Must(BeAValidPassword);
    }



    private static bool BeAValidPassword(string password)
    {
        return password is { Length: >= 8 } &&
               password.Any(char.IsUpper) &&
               password.Any(char.IsLower) &&
               password.Any(ch => !char.IsLetterOrDigit(ch));
    }
}
