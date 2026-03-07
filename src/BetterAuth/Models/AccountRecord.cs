namespace BetterAuth.Models;

public record AccountRecord
{
    public required string Id { get; init; }
    public required string UserId { get; init; }
    public required string AccountId { get; init; }
    public required string ProviderId { get; init; }
    public required string? Password { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
};