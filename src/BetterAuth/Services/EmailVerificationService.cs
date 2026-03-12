using BetterAuth.Abstractions;
using BetterAuth.Configuration;
using BetterAuth.Core;
using BetterAuth.Models;
using BetterAuth.Models.Inputs;
using BetterAuth.Templates;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BetterAuth.Services;

public class EmailVerificationService(BetterAuthEngine engine)
{
    public async Task SendVerificationAsync(UserRecord user, string callbackUrl, HttpContext context)
    {
        var callback = engine.Options.EmailVerification.SendVerificationEmail;
        
        var verification = await engine.InternalAdapter.CreateVerificationValueAsync(new CreateVerificationInput
        {
            Identifier = user.Email,
            ExpiresAt = DateTime.UtcNow.Add(engine.Options.EmailVerification.ExpiresIn)
        });
        
        var baseUrl = engine.Options.BaseUrl ?? Environment.GetEnvironmentVariable("BETTER_AUTH_URL") ??
            $"{context.Request.Scheme}://{context.Request.Host}";
        
        var url =
            $"{baseUrl}{engine.Options.BasePath}/verify-email?token={Uri.EscapeDataString(verification.Value)}&callbackURL={Uri.EscapeDataString(string.IsNullOrEmpty(callbackUrl) ? baseUrl : callbackUrl)}";

        var data = new SendVerificationEmailData(user, url, verification.Value);

        if (callback is not null)
        {
            await callback(data, context);
            return;
        }
        
        var emailProvider = context.RequestServices.GetService<IEmailProvider>();
        var emailOptions = engine.Options.Email;

        if (emailProvider is null) return;

        if (emailOptions is null) throw new Exception("Email option must be configured if using an email provider.");

        var template = engine.Options.EmailVerification.Template ??
                       new DefaultVerificationTemplate(emailOptions.Branding);

        await emailProvider.SendAsync(new EmailMessage
        {
            From = emailOptions.DefaultFrom,
            To = user.Email,
            Subject = template.Subject,
            Html = template.Render(new Dictionary<string, string>()
            {
                ["userName"] = user.Name,
                ["verificationUrl"] = url,
                ["appName"] = emailOptions.Branding.AppName
            }),
        });

        // if (callback is null)
        // {
        //     return;
        // }
        //
        // var verification = await engine.InternalAdapter.CreateVerificationValueAsync(new CreateVerificationInput
        // {
        //     Identifier = user.Email,
        //     ExpiresAt = DateTime.UtcNow.Add(engine.Options.EmailVerification.ExpiresIn)
        // });
        //
        // var baseUrl = engine.Options.BaseUrl ?? Environment.GetEnvironmentVariable("BETTER_AUTH_URL") ??
        //     $"{context.Request.Scheme}://{context.Request.Host}";
        //
        // var url =
        //     $"{baseUrl}{engine.Options.BasePath}/verify-email?token={Uri.EscapeDataString(verification.Value)}&callbackURL={Uri.EscapeDataString(string.IsNullOrEmpty(callbackUrl) ? baseUrl : callbackUrl)}";
        //
        // await callback(new SendVerificationEmailData(user, url, verification.Value), context);
    }
}