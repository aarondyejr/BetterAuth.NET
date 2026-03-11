namespace BetterAuth.Database.Args;

public record CreateArgs
{
    public required string Model { get; init; }
    public required Dictionary<string, object?> Data { get; init; }
    public string[]? Select { get; init; }
}
