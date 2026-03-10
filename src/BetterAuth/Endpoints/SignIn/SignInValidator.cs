using BetterAuth.Configuration;
using FluentValidation;

namespace BetterAuth.Endpoints.SignIn;

public class SignInValidator : AbstractValidator<SignInRequest>
{
    public  SignInValidator()
    {
        RuleFor(r => r.Email).NotEmpty().WithMessage("Email is required");
        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}