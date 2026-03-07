namespace BetterAuth.Adapters.Args;

public record FindManyArgs
{
    public required string Model { get; init; }
    public string[]? Select { get; init; }
    public List<WhereClause>? Where { get; init; }
    public int? Limit { get; init; }
    public int? Offset { get; init; }
    public SortBy? SortBy { get; init; }
}
