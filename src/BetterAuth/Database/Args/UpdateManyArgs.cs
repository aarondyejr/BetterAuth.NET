namespace BetterAuth.Database.Args;

public record UpdateManyArgs
{
    public required string Model { get; init; }
    public required List<WhereClause> Where { get; init; }
    public required Dictionary<string, object?> Data { get; init; }
}
