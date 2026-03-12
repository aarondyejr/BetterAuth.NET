using BetterAuth.Abstractions;
using BetterAuth.Configuration;

namespace BetterAuth.Templates;

public class DefaultVerificationTemplate(EmailBranding branding) : IEmailTemplate
{
    public string Subject => $"Verify your email - {branding.AppName}";

    public string Render(Dictionary<string, string> variables)
    {
        return $"""
                <div style="font-family: sans-serif; max-width: 600px; margin: 0 auto;">
                    {(branding.LogoUrl is not null ? $"<img src=\"{branding.LogoUrl}\" alt=\"{branding.AppName}\" />" : "")}
                    <h1 style="color: {branding.PrimaryColor};">Verify your email</h1>
                    <p>Hi {variables["userName"]},</p>
                    <p>Click the button below to verify your email address.</p>
                    <a href="{variables["verificationUrl"]}" 
                       style="background: {branding.PrimaryColor}; color: white; padding: 12px 24px; 
                              text-decoration: none; border-radius: 4px; display: inline-block;">
                        Verify Email
                    </a>
                </div>
                """;
    }
}