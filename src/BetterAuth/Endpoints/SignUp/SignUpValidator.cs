using BetterAuth.Configuration;
using FluentValidation;

namespace BetterAuth.Endpoints.SignUp;

public class SignUpValidator : AbstractValidator<SignUpRequest>
{
    public SignUpValidator(EmailPasswordOptions options)
    {
        RuleFor(r => r.Email).NotEmpty().WithMessage("Email is required").EmailAddress().WithMessage("Invalid email");
        RuleFor(r => r.Name).NotEmpty().WithMessage("Name is required").MaximumLength(100).WithMessage("Name cannot exceed 100 characters");
        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(options.MinPasswordLength)
            .WithMessage($"Password must be at least {options.MinPasswordLength} characters")
            .MaximumLength(options.MaxPasswordLength)
            .WithMessage($"Password cannot exceed {options.MaxPasswordLength} characters");
    }
}