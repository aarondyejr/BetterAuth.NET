namespace BetterAuth.Adapters.Args;

public record DeleteArgs
{
    public required string Model { get; init; }
    public required List<WhereClause> Where { get; init; }
}
