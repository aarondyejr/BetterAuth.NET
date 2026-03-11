using BetterAuth.Abstractions;
using BetterAuth.Database;
using BetterAuth.Postgres.MySql;
using BetterAuth.Postgres.Postgres;
using BetterAuth.Postgres.Sqlite;

namespace BetterAuth.Postgres;

public static class BetterAuthDatabase
{
    public static IAuthDatabaseAdapter Postgres(string connectionString)
    {
        return AuthDatabaseAdapterFactory.Create(config: new()
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

    public static IAuthDatabaseAdapter Sqlite(string connectionString)
    {
        return AuthDatabaseAdapterFactory.Create(config: new()
        {
            AdapterId = "sqlite",
            AdapterName = "Sqlite",
            SupportsJson = false,
            SupportsDates = false,
            SupportsBooleans = false,
            SupportsJoin = true,
            SupportsTransactions = true,
        }, rawAdapter: new SqliteAdapter(connectionString));
    }

    public static IAuthDatabaseAdapter MySql(string connectionString)
    {
        return AuthDatabaseAdapterFactory.Create(config: new()
        {
            AdapterId = "mysql",
            AdapterName = "MySQL",
            SupportsJson = true,
            SupportsDates = true,
            SupportsBooleans = true,
            SupportsJoin = true,
            SupportsTransactions = true,
        }, rawAdapter: new MySqlAdapter(connectionString));
    }
}
