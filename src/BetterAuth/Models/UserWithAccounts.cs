namespace BetterAuth.Models;

public record UserWithAccounts : UserRecord
{
    public required List<AccountRecord> Accounts { get; init; }
};