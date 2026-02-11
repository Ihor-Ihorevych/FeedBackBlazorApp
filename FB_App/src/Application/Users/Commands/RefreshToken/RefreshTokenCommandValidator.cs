namespace FB_App.Application.Users.Commands.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(v => v.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
