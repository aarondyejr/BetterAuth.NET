using BetterAuth.Abstractions;

namespace BetterAuth.Configuration;

public class BetterAuthOptions
{
    public string BasePath { get; init; } = "/api/auth";
    
    public string? Secret { get; init; }
    
    public string? BaseUrl { get; init; }
    
    public string[]? TrustedOrigins { get; init; }
    
    public required IAuthDatabaseAdapter DatabaseAdapter { get; init; }

    public List<IBetterAuthPlugin> Plugins { get; init; } = [];

    public SessionOptions Session { get; init; } = new()
    {
        ExpiresIn = TimeSpan.FromDays(7),
        UpdateAge = TimeSpan.FromDays(1)
    };

    public EmailPasswordOptions EmailAndPassword { get; init; } = new();
}