namespace BetterAuth.Plugins;

public class PluginSchema
{
    public Dictionary<string, ModelSchema> Models { get; init; } = new();
}

public record ModelSchema
{
    public Dictionary<string, FieldSchema> Fields { get; init; } = new();

    public bool IsNewModel { get; set; } = false;
}

public record FieldSchema
{
    public required FieldType Type { get; init; }
    public bool Required { get; init; } = false;
    public bool Unique { get; init; } = false;
    public object? DefaultValue { get; init; }
    public string? References { get; init; }
}

public enum FieldType
{
    String,
    Number,
    Boolean,
    Date
}