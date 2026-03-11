namespace BetterAuth.Database;

public record AdapterConfig
{
    public required string AdapterId { get; init; }
    public required string AdapterName { get; init; }
    public bool SupportsJson { get; init; } = false;
    public bool SupportsDates { get; init; } = true;
    public bool SupportsBooleans { get; init; } = true;
    public bool SupportsNumericIds { get; init; } = true;
    public bool UsePlural { get; init; } = false;
    public bool SupportsJoin { get; init; } = false;
    public bool SupportsTransactions { get; init; } = false;
    
    public Dictionary<string, string>? MapKeysInput { get; init; }
    public Dictionary<string, string>? MapKeysOutput { get; init; }

    public Func<string, object?, object?>? CustomTransformInput { get; init; }
    public Func<string, object?, object?>? CustomTransformOutput { get; init; }
};