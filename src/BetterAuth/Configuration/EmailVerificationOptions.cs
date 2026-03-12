using BetterAuth.Abstractions;
using BetterAuth.Models;
using Microsoft.AspNetCore.Http;

namespace BetterAuth.Configuration;

public record SendVerificationEmailData(UserRecord User, string Url, string Token);

public class EmailVerificationOptions
{
    public Func<SendVerificationEmailData, HttpContext, Task>? SendVerificationEmail { get; init; }
    public IEmailTemplate? Template { get; init; }
    public bool SendOnSignUp { get; init; } = true;
    public bool SendOnSignIn { get; init; } = false;
    
    public bool AutoSignInAfterVerification { get; init; } = false;
    public TimeSpan ExpiresIn { get; init; } = TimeSpan.FromHours(1);
}