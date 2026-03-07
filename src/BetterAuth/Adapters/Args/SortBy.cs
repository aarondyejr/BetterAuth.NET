namespace BetterAuth.Adapters.Args;

public record SortBy
{
    public required string Field { get; init; }
    public SortDirection Direction { get; init; } = SortDirection.Ascending;
}

public enum SortDirection
{
    Ascending,
    Descending
}
