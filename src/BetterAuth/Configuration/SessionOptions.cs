using Microsoft.AspNetCore.Http;

namespace BetterAuth.Configuration;

public class SessionOptions
{
    public TimeSpan ExpiresIn { get; init; } = TimeSpan.FromDays(7);
    public TimeSpan? UpdateAge { get; init; }

    public CookieOptions Cookie { get; init; } = new()
    {
        SameSite = SameSiteMode.Lax,
        Secure = true,
        HttpOnly = true,
    };
}