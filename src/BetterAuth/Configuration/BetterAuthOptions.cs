using BetterAuth.Abstractions;
using BetterAuth.Events;

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

    public RateLimitOptions RateLimit { get; init; } = new();
    
    public AuthEventOptions? Events { get; init; }

    public EmailPasswordOptions EmailAndPassword { get; init; } = new();
    
    public EmailVerificationOptions EmailVerification { get; init; } = new();
    
    public EmailOptions? Email { get; init; }
    
    public string SessionCookieName { get; init; } = "better-auth.session_token";
}