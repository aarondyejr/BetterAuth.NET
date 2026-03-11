namespace BetterAuth.Database.Args;

public record DeleteManyArgs
{
    public required string Model { get; init; }
    public required List<WhereClause> Where { get; init; }
}
