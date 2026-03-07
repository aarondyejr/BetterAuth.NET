namespace BetterAuth.Models.Inputs;

public record CreateVerificationInput
{
    public required string Identifier { get; init; }
    public required string Value { get; init; }
    public required DateTime ExpiresAt { get; init; }
};