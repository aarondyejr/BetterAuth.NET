namespace BetterAuth.Configuration;

public class SessionOptions
{
    public TimeSpan ExpiresIn { get; init; } = TimeSpan.FromDays(7);
    public TimeSpan? UpdateAge { get; init; }
}