using BetterAuth.Abstractions;
using BetterAuth.Database.Args;
using BetterAuth.Plugins;
using Dapper;
using Microsoft.Data.Sqlite;

namespace BetterAuth.Adapters.Sqlite;

public class SqliteAdapter(string connectionString) : BaseSqlAdapter
{
    protected override SqliteConnection CreateConnection() => new(connectionString);
    protected override string Quote(string identifier) => $"\"{identifier}\"";
    protected override bool SupportsReturning => false;
    protected override string MapFieldType(FieldType type) => type switch
    {
        FieldType.String => "TEXT",
        FieldType.Boolean => "INTEGER",
        FieldType.Number => "INTEGER",
        FieldType.Date => "TEXT",
        _ => throw new InvalidOperationException("FieldType not supported")
    };
}