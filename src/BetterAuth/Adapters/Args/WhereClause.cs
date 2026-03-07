namespace BetterAuth.Adapters.Args;

public record WhereClause
{
    public required string Field { get; init; }
    public required WhereOperator Operator { get; init; }
    public object? Value { get; init; }
}

public enum WhereOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    GreaterThanOrEquals,
    LessThan,
    LessThanOrEquals,
    Contains,
    StartsWith,
    EndsWith,
    In
}
