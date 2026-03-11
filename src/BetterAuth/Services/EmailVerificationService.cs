using BetterAuth.Configuration;
using BetterAuth.Core;
using BetterAuth.Models;
using BetterAuth.Models.Inputs;
using Microsoft.AspNetCore.Http;

namespace BetterAuth.Services;

public class EmailVerificationService(BetterAuthEngine engine)
{
    public async Task SendVerificationAsync(UserRecord user, string callbackUrl, HttpContext context)
    {
        var callback = engine.Options.EmailVerification.SendVerificationEmail;

        if (callback is null)
        {
            return;
        }

        var verification = await engine.InternalAdapter.CreateVerificationValueAsync(new CreateVerificationInput
        {
            Identifier = user.Email,
            ExpiresAt = DateTime.UtcNow.Add(engine.Options.EmailVerification.ExpiresIn)
        });

        var baseUrl = engine.Options.BaseUrl ?? Environment.GetEnvironmentVariable("BETTER_AUTH_URL") ??
            $"{context.Request.Scheme}://{context.Request.Host}";

        var url =
            $"{baseUrl}{engine.Options.BasePath}/verify-email?token={Uri.EscapeDataString(verification.Value)}&callbackURL={Uri.EscapeDataString(string.IsNullOrEmpty(callbackUrl) ? baseUrl : callbackUrl)}";
        
        await callback(new SendVerificationEmailData(user, url, verification.Value), context);
    }
}