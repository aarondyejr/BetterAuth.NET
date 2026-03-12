using BetterAuth.Abstractions;

namespace BetterAuth.Configuration;

public class EmailOptions
{
    public string DefaultFrom { get; init; } = "noreply@example.com";
    public EmailBranding Branding { get; init; } = new();
}

public class EmailBranding
{
    public string AppName { get; init; } = "BetterAuth";
    public string? LogoUrl { get; init; }
    public string PrimaryColor { get; init; } = "#007bff";
}