namespace BetterAuth.Models.Inputs;

public record SessionMetadata
{
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}