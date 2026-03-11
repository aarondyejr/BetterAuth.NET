using BetterAuth.Abstractions;
using BetterAuth.Database.Args;
using BetterAuth.Plugins;
using Dapper;
using MySqlConnector;

namespace BetterAuth.Postgres.MySql;

public class MySqlAdapter(string connectionString) : IAuthDatabaseAdapter
{
        private readonly string _connectionString = NormalizeConnectionString(connectionString);

    private MySqlConnection CreateConnection() => new(_connectionString);
    
    public async Task<Dictionary<string, object?>> CreateAsync(CreateArgs args)
    {
        await using var conn = CreateConnection();
    
        var columns = string.Join(", ", args.Data.Keys.Select(k => $"`{k}`"));
        var parameters = string.Join(", ", args.Data.Keys.Select(k => $"@{k}"));
    
        var sql = $"INSERT INTO `{args.Model}` ({columns}) VALUES ({parameters});";
        await conn.ExecuteAsync(sql, args.Data);

        var selectSql = $"SELECT * FROM `{args.Model}` WHERE `id` = @id";
        var result = await conn.QuerySingleAsync(selectSql, new { id = args.Data["id"] });

        return ToDictionary(result);
    }

    public async Task<Dictionary<string, object?>> UpdateAsync(UpdateArgs args)
    {
        await using var conn = CreateConnection();

        var setClauses = string.Join(", ", args.Data.Keys.Select(k => $"`{k}` = @{k}"));
        var (whereClause, whereParams) = BuildWhereClause(args.Where);

        foreach (var key in args.Data)
        {
            whereParams.Add(key.Key, key.Value);
        }

        var updateSql = $"UPDATE `{args.Model}` SET {setClauses} {whereClause}";
        await conn.ExecuteAsync(updateSql, whereParams);
        
        var selectSql = $"SELECT * FROM `{args.Model}` {whereClause}";
        var result = await conn.QuerySingleAsync(selectSql, whereParams);
        
        return  ToDictionary(result);
    }

    public async Task<int> UpdateManyAsync(UpdateManyArgs args)
    {
        await using var conn = CreateConnection();

        var setClauses = string.Join(", ", args.Data.Keys.Select(k => $"`{k}` = @{k}"));
        var (whereClause, whereParams) = BuildWhereClause(args.Where);

        foreach (var key in args.Data)
        {
            whereParams.Add(key.Key, key.Value);
        }

        var sql = $"UPDATE `{args.Model}` SET {setClauses} {whereClause}";
        
        return await conn.ExecuteAsync(sql, whereParams);
    }

    public async Task<bool> DeleteAsync(DeleteArgs args)
    {
        await using var conn = CreateConnection();
        
        var (whereClause, whereParams) = BuildWhereClause(args.Where);

        var sql = $"DELETE FROM `{args.Model}` {whereClause}";
        
        var result = await conn.ExecuteAsync(sql, whereParams);

        return result > 0;
    }

    public async Task<int> DeleteManyAsync(DeleteManyArgs args)
    {
        await using var conn = CreateConnection();
        
        var (whereClause, whereParams) = BuildWhereClause(args.Where);

        var sql = $"DELETE FROM `{args.Model}` {whereClause}";
        
        return await conn.ExecuteAsync(sql, whereParams);
    }
    
    public async Task<Dictionary<string, object?>?> FindOneAsync(FindOneArgs args)
    {
        await using var conn = CreateConnection();
        var (whereClause, whereParams) = BuildWhereClause(args.Where);
        
        var select = args.Select != null
            ? string.Join(", ", args.Select.Select(s => $"`{s}`"))
            : "*";

        var sql = $"SELECT {select} FROM `{args.Model}` {whereClause} LIMIT 1";

        return ToDictionary(await conn.QuerySingleOrDefaultAsync(sql, whereParams));
    }

    public async Task<List<Dictionary<string, object?>>> FindManyAsync(FindManyArgs args)
    {
        await using var conn = CreateConnection();
        var (whereClause, whereParams) = BuildWhereClause(args.Where ?? []);

        var select = args.Select != null
            ? string.Join(", ", args.Select.Select(s => $"`{s}`"))
            : "*";

        var sql = $"SELECT {select} FROM `{args.Model}` {whereClause}";

        if (args.SortBy != null)
        {
            var direction = args.SortBy.Direction == SortDirection.Ascending ? "ASC" : "DESC";
            sql += $" ORDER BY `{args.SortBy.Field}` {direction}";
        }

        if (args.Limit.HasValue)
            sql += $" LIMIT {args.Limit.Value}";

        if (args.Offset.HasValue)
            sql += $" OFFSET {args.Offset.Value}";

        var results = await conn.QueryAsync(sql, whereParams);

        return results.Select(r => (Dictionary<string, object?>)ToDictionary((dynamic?)r)!).ToList();
    }

    public async Task<int> CountAsync(CountArgs args)
    {
        await using var conn = CreateConnection();
        
        var (whereClause, whereParams) = BuildWhereClause(args.Where ?? []);
        var sql = $"SELECT COUNT(*) FROM `{args.Model}` {whereClause}";
        
        var result = await conn.QuerySingleAsync<int>(sql, whereParams);

        return result;
    }

    public async Task<T> TransactionAsync<T>(Func<IAuthDatabaseAdapter, Task<T>> operation)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        
        await using var transaction = await conn.BeginTransactionAsync();

        try
        {
            var result = await operation(this);
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task TransactionAsync(Func<IAuthDatabaseAdapter, Task> operation)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        
        await using var transaction = await conn.BeginTransactionAsync();

        try
        {
            await operation(this);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public string GenerateMigrationSql(PluginSchema schema)
    {
        var sql = "";
        foreach (var model in schema.Models)
        {
            var tableName = model.Key;
            sql += $"CREATE TABLE IF NOT EXISTS `{tableName}` (\n";
        
            var columns = new List<string>();
            foreach (var field in model.Value.Fields)
            {
                var fieldName = field.Key;
                var value = field.Value;
                var type = value.Type switch
                {
                    FieldType.String => "LONGTEXT", 
                    FieldType.Boolean => "TINYINT(1)",
                    FieldType.Number => "INT",
                    FieldType.Date => "DATETIME(3)",
                    _ => throw new InvalidOperationException("FieldType not supported")
                };

                var column = $"`{fieldName}` {type}";

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

    public async Task ExecuteMigrationAsync(string sql)
    {
        await using var conn = CreateConnection();
        
        await conn.ExecuteAsync(sql);
    }

    private (string sql, DynamicParameters parameters) BuildWhereClause(List<WhereClause> clauses)
    {
        var parameters = new DynamicParameters();
        var conditions = new List<string>();

        for (var i = 0; i < clauses.Count; i++)
        {
            var clause = clauses[i];
            var paramName = $"p{i}";
            var op = clause.Operator switch
            {
                WhereOperator.Equals => "=",
                WhereOperator.NotEquals => "!=",
                WhereOperator.GreaterThan => ">",
                WhereOperator.GreaterThanOrEquals => ">=",
                WhereOperator.LessThan => "<",
                WhereOperator.LessThanOrEquals => "<=",
                WhereOperator.Contains => "LIKE",
                WhereOperator.StartsWith => "LIKE",
                WhereOperator.EndsWith => "LIKE",
                _ => throw new ArgumentException($"Unsupported operator: {clause.Operator}")
            };
            
            conditions.Add($"`{clause.Field}` {op} @{paramName}");
            parameters.Add(paramName, clause.Operator switch
            {
                WhereOperator.Contains => $"%{clause.Value}%",
                WhereOperator.StartsWith => $"{clause.Value}%",
                WhereOperator.EndsWith => $"%{clause.Value}",
                _ => clause.Value
            });
        }
        
        var sql = conditions.Count > 0
            ? $"WHERE {string.Join(" AND ", conditions)}"
            : "";

        return (sql, parameters);
    }
    
    private Dictionary<string, object?>? ToDictionary(dynamic? result)
    {
        return result is null 
            ? null 
            : ((IDictionary<string, object?>)result).ToDictionary(x => x.Key, x => x.Value);
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