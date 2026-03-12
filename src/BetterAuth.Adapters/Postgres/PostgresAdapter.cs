using BetterAuth.Plugins;
using Npgsql;

namespace BetterAuth.Adapters.Postgres;

public class PostgresAdapter(string connectionString) : BaseSqlAdapter
{
    private readonly string _connectionString = NormalizeConnectionString(connectionString);

    protected override NpgsqlConnection CreateConnection() => new(_connectionString);
    protected override string Quote(string identifier) => $"\"{identifier}\"";

    protected override bool SupportsReturning => true;

    protected override string MapFieldType(FieldType type) => type switch
    {
        FieldType.String => "TEXT",
        FieldType.Boolean => "BOOLEAN",
        FieldType.Number => "INTEGER",
        FieldType.Date => "TIMESTAMP",
        _ => throw new InvalidOperationException("FieldType not supported")
    };

    public override string GenerateMigrationSql(PluginSchema schema)
    {
        var sql = "";

        foreach (var model in schema.Models)
        {
            var tableName = model.Key; // e.g user

            sql += $"CREATE TABLE IF NOT EXISTS \"{tableName}\" ";
            
            var columns = new List<string>();

            foreach (var field in model.Value.Fields)
            {
                var column = "";
                var fieldName = field.Key; // e.g id
                var value = field.Value;

                var type = value.Type switch
                {
                    FieldType.String => "TEXT",
                    FieldType.Boolean => "BOOLEAN",
                    FieldType.Number => "INTEGER",
                    FieldType.Date => "TIMESTAMP",
                    _ => throw new InvalidOperationException("FieldType not supported")
                };

                if (fieldName == "id")
                    column += $"\"{fieldName}\" {type} PRIMARY KEY";
                else
                {
                    column += $"\"{fieldName}\" {type} ";

                    if (value.Required) column += "NOT NULL ";
                    if (value.Unique) column += "UNIQUE ";
                    if (value.DefaultValue != null) column += $"DEFAULT {value.DefaultValue} ";
                    if (value.References != null) column += $"REFERENCES \"{value.References}\"(\"id\")";
                }
                
                columns.Add(column);
            }
            
            sql += $"({string.Join(",\n", columns)});\n\n";
        }

        return sql;
    }
    
    private static string NormalizeConnectionString(string connectionString)
    {
        if (!connectionString.StartsWith("postgresql://") && !connectionString.StartsWith("postgres://"))
            return connectionString; // already in key-value format

        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':');
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
    
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = userInfo[0],
            Password = userInfo.Length > 1 ? userInfo[1] : null,
        };

        // Handle sslmode from query string
        var sslMode = query["sslmode"];
        if (!string.IsNullOrEmpty(sslMode))
        {
            builder.SslMode = sslMode.ToLower() switch
            {
                "require" => SslMode.Require,
                "prefer" => SslMode.Prefer,
                "disable" => SslMode.Disable,
                _ => SslMode.Require
            };
        }

        return builder.ConnectionString;
    }
}