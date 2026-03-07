namespace BetterAuth.Models;

public record SessionRecord
{
    public required string Id { get; init; }
    public required string UserId { get; init; }
    public required string Token { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
};