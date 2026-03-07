namespace BetterAuth.Models;


public record UserRecord
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required bool EmailVerified { get; init; }
    public string? Image { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}