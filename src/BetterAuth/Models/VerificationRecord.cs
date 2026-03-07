namespace BetterAuth.Models;

public record VerificationRecord
{
    public required string Id { get; init; }
    public required string Identifier { get; init; }
    public required string Value { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
};