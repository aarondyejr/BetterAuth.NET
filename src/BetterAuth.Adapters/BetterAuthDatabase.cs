using BetterAuth.Abstractions;
using BetterAuth.Adapters;
using BetterAuth.Postgres.Postgres;

namespace BetterAuth.Postgres;

public static class BetterAuthDatabase
{
    public static IAuthDatabaseAdapter Postgres(string connectionString)
    {
        return AuthDatabaseAdapterFactory.Create(config: new AdapterConfig
        {
            AdapterId = "postgres",
            AdapterName = "PostgreSQL",
            SupportsJson = true,
            SupportsDates = true,
            SupportsBooleans = true,
            SupportsJoin = true,
            SupportsTransactions = true,
        }, rawAdapter: new PostgresAdapter(connectionString));
    }
}
