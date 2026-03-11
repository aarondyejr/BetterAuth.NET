namespace BetterAuth.Configuration;

public class RateLimitOptions
{
    public TimeSpan Window { get; init; } = TimeSpan.FromMinutes(1);
    public int MaxRequests { get; init; } = 100;
    public RateLimitKeyType KeyType { get; init; } = RateLimitKeyType.Ip;
}

public enum RateLimitKeyType
{
    Ip,
    UserId,
    IpAndUserId
}