using BetterAuth.Plugins;
using MySqlConnector;

namespace BetterAuth.Adapters.MySql;

public class MySqlAdapter(string connectionString) : BaseSqlAdapter
{
        private readonly string _connectionString = NormalizeConnectionString(connectionString);

    protected override MySqlConnection CreateConnection() => new(_connectionString);
    protected override string Quote(string identifier) => $"`{identifier}`";

    protected override bool SupportsReturning => false;
    
    protected override string MapFieldType(FieldType type) => type switch
    {
        FieldType.String => "LONGTEXT", 
        FieldType.Boolean => "TINYINT(1)",
        FieldType.Number => "INT",
        FieldType.Date => "DATETIME(3)",
        _ => throw new InvalidOperationException("FieldType not supported")
    };

    public override string GenerateMigrationSql(PluginSchema schema)
    {
        var sql = "";
        foreach (var (tableName, modelSchema) in schema.Models)
        {
            sql += $"CREATE TABLE IF NOT EXISTS `{tableName}` (\n";
        
            var columns = new List<string>();
            foreach (var field in modelSchema.Fields)
            {
                var fieldName = field.Key;
                var value = field.Value;

                var column = $"`{fieldName}` {MapFieldType(field.Value.Type)}";

                if (fieldName == "id") column += " PRIMARY KEY";
                if (value.Required) column += " NOT NULL";
                if (value.Unique) column += " UNIQUE";
                if (value.DefaultValue != null) column += $" DEFAULT {value.DefaultValue}";
                if (value.References != null) column += $" , FOREIGN KEY (`{fieldName}`) REFERENCES `{value.References}`(`id`)";
            
                columns.Add(column);
            }
            sql += $"{string.Join(",\n", columns)}\n) ENGINE=InnoDB;\n\n";
        }
        return sql;
    }

    private static string NormalizeConnectionString(string connectionString)
    {
        // MySQL connection strings usually start with mysql://
        if (!connectionString.StartsWith("mysql://"))
            return connectionString; // already in key-value format or unknown format

        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':');
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

        var builder = new MySqlConnectionStringBuilder
        {
            Server = uri.Host,
            Port = (uint)(uri.Port > 0 ? uri.Port : 3306),
            Database = uri.AbsolutePath.TrimStart('/'),
            UserID = userInfo[0],
            Password = userInfo.Length > 1 ? userInfo[1] : null,
        };

        // Handle sslmode from query string
        var sslMode = query["sslmode"];
        if (!string.IsNullOrEmpty(sslMode))
        {
            builder.SslMode = sslMode.ToLower() switch
            {
                "required" or "require" => MySqlSslMode.Required,
                "preferred" or "prefer" => MySqlSslMode.Preferred,
                "none" or "disable"    => MySqlSslMode.None,
                "verifyca"             => MySqlSslMode.VerifyCA,
                "verifyfull"           => MySqlSslMode.VerifyFull,
                _                      => MySqlSslMode.Required
            };
        }

        return builder.ConnectionString;
    }
}