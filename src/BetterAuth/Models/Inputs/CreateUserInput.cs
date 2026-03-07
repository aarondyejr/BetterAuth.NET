namespace BetterAuth.Models.Inputs;

public record CreateUserInput
{
    public required string Email { get; init; }
    public required string Name { get; init; }
    public string? Image { get; init; }
};