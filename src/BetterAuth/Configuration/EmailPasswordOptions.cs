namespace BetterAuth.Configuration;

public class EmailPasswordOptions
{
    public bool Enabled { get; init; } = true;
    public int MinPasswordLength { get; init; } = 8;
    public int MaxPasswordLength { get; init; } = 128;
}