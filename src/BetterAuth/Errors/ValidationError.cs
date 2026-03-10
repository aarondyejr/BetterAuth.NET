namespace BetterAuth.Errors;

public record ValidationError
{
    public string Field { get; init; } = "";
    public string Message { get; init; } = "";
}