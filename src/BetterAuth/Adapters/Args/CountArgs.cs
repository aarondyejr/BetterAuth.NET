namespace BetterAuth.Adapters.Args;

public record CountArgs
{
    public required string Model { get; init; }
    public List<WhereClause>? Where { get; init; }
}
