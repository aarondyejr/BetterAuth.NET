namespace BetterAuth.Configuration;

public class EmailMessage
{
    public required string From { get; init; }
    public required string To { get; init; }
    public required string Subject { get; init; }
    public string? Html { get; init; }
    public string? Text { get; init; }
}