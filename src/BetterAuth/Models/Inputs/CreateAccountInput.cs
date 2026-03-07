namespace BetterAuth.Models.Inputs;

public record CreateAccountInput
{
    public required string UserId { get; init; }
    public required string AccountId { get; init; }
    public required string ProviderId { get; init; }
    public required string? Password { get; init; }
};