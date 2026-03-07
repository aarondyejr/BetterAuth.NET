namespace BetterAuth.Adapters.Args;

public record FindOneArgs
{
    public required string Model { get; init; }
    public required List<WhereClause> Where { get; init; }
    public string[]? Select { get; init; }
}
