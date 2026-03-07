namespace BetterAuth.Plugins;

public class AuthRequestSchema
{
    public Dictionary<string, FieldValidation> Fields { get; init; } = new();
}

public class FieldValidation
{
    public required FieldType Type { get; init; }
    public bool Required { get; init; } = true;
    public string? Description { get; init; }
    public string[]? AllowedValues { get; init; } // for enum-like fields
}