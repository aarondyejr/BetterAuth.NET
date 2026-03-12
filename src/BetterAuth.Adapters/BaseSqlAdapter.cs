using System.Data.Common;
using BetterAuth.Abstractions;
using BetterAuth.Database.Args;
using BetterAuth.Plugins;
using Dapper;

namespace BetterAuth.Adapters;

public abstract class BaseSqlAdapter : IAuthDatabaseAdapter
{
    protected abstract DbConnection CreateConnection();
    protected abstract string Quote(string identifier);
    protected abstract bool SupportsReturning { get; }
    protected abstract string MapFieldType(FieldType type);

    public virtual async Task<Dictionary<string, object?>> CreateAsync(CreateArgs args)
    {
        await using var conn = CreateConnection();

        var columns = string.Join(", ", args.Data.Keys.Select(Quote));
        var parameters = string.Join(", ", args.Data.Keys.Select(k => $"@{k}"));
        var select = args.Select != null ? string.Join(", ", args.Select.Select(Quote)) : "*";

        if (SupportsReturning)
        {
            var sql = $"INSERT INTO {Quote(args.Model)} ({columns}) VALUES ({parameters}) RETURNING {select}";
            return ToDictionary(await conn.QuerySingleAsync(sql, args.Data))!;
        }

        await conn.ExecuteAsync($"INSERT INTO {Quote(args.Model)} ({columns}) VALUES ({parameters})", args.Data);
        var result = await conn.QuerySingleAsync($"SELECT * FROM {Quote(args.Model)} WHERE {Quote("id")} = @id", new { id = args.Data["id"] });
        return ToDictionary(result)!;
    }

    public virtual async Task<Dictionary<string, object?>> UpdateAsync(UpdateArgs args)
    {
        await using var conn = CreateConnection();

        var setClauses = string.Join(", ", args.Data.Keys.Select(k => $"{Quote(k)} = @{k}"));
        var (whereClause, whereParams) = BuildWhereClause(args.Where);

        foreach (var key in args.Data)
            whereParams.Add(key.Key, key.Value);

        if (SupportsReturning)
        {
            var sql = $"UPDATE {Quote(args.Model)} SET {setClauses} {whereClause} RETURNING *";
            return ToDictionary(await conn.QuerySingleAsync(sql, whereParams))!;
        }

        await conn.ExecuteAsync($"UPDATE {Quote(args.Model)} SET {setClauses} {whereClause}", whereParams);
        var result = await conn.QuerySingleAsync($"SELECT * FROM {Quote(args.Model)} {whereClause}", whereParams);
        return ToDictionary(result)!;
    }

    public virtual async Task<int> UpdateManyAsync(UpdateManyArgs args)
    {
        await using var conn = CreateConnection();
        
        var setClauses = string.Join(", ", args.Data.Keys.Select(k => $"{Quote(k)} = @{k}"));
        var (whereClause, whereParams) = BuildWhereClause(args.Where);

        foreach (var key in args.Data)
        {
            whereParams.Add(key.Key, key.Value);
        }

        var sql = $"UPDATE {Quote(args.Model)} SET {setClauses} {whereClause}";
        
        return await conn.ExecuteAsync(sql, whereParams);
    }

    public virtual async Task<bool> DeleteAsync(DeleteArgs args)
    {
        await using var conn = CreateConnection();
        
        var (whereClause, whereParams) = BuildWhereClause(args.Where);
        
        var sql = $"DELETE FROM {Quote(args.Model)} {whereClause}";
        
        var result = await conn.ExecuteAsync(sql, whereParams);

        return result > 0;
    }

    public virtual async Task<int> DeleteManyAsync(DeleteManyArgs args)
    {
        await using var conn = CreateConnection();
        
        var (whereClause, whereParams) = BuildWhereClause(args.Where);

        var sql = $"DELETE FROM {Quote(args.Model)} {whereClause}";
        
        return await conn.ExecuteAsync(sql, whereParams);
    }
    
    public async Task<Dictionary<string, object?>?> FindOneAsync(FindOneArgs args)
    {
        await using var conn = CreateConnection();
        var (whereClause, whereParams) = BuildWhereClause(args.Where);
        
        var select = args.Select != null
            ? string.Join(", ", args.Select.Select(Quote))
            : "*";

        var sql = $"SELECT {select} FROM {Quote(args.Model)} {whereClause} LIMIT 1";

        return ToDictionary(await conn.QuerySingleOrDefaultAsync(sql, whereParams));
    }
    
    public virtual async Task<List<Dictionary<string, object?>>> FindManyAsync(FindManyArgs args)
    {
        await using var conn = CreateConnection();
        var (whereClause, whereParams) = BuildWhereClause(args.Where ?? []);

        var select = args.Select != null
            ? string.Join(", ", args.Select.Select(Quote))
            : "*";

        var sql = $"SELECT {select} FROM {Quote(args.Model)} {whereClause}";

        if (args.SortBy != null)
        {
            var direction = args.SortBy.Direction == SortDirection.Ascending ? "ASC" : "DESC";
            sql += $" ORDER BY {Quote(args.SortBy.Field)} {direction}";
        }

        if (args.Limit.HasValue)
            sql += $" LIMIT {args.Limit.Value}";

        if (args.Offset.HasValue)
            sql += $" OFFSET {args.Offset.Value}";

        var results = await conn.QueryAsync(sql, whereParams);

        return results.Select(r => (Dictionary<string, object?>)ToDictionary((dynamic?)r)!).ToList();
    }
    
    public virtual async Task<int> CountAsync(CountArgs args)
    {
        await using var conn = CreateConnection();
        
        var (whereClause, whereParams) = BuildWhereClause(args.Where ?? []);
        var sql = $"SELECT COUNT(*) FROM {Quote(args.Model)} {whereClause}";
        
        var result = await conn.QuerySingleAsync<int>(sql, whereParams);

        return result;
    }
    
    public virtual async Task<T> TransactionAsync<T>(Func<IAuthDatabaseAdapter, Task<T>> operation)
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
    
    public virtual async Task TransactionAsync(Func<IAuthDatabaseAdapter, Task> operation)
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
            
        conditions.Add($"{Quote(clause.Field)} {op} @{paramName}");
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
    
    public virtual string GenerateMigrationSql(PluginSchema schema)
    {
        var sql = "";

        foreach (var model in schema.Models)
        {
            sql += $"CREATE TABLE IF NOT EXISTS {Quote(model.Key)} ";
            var columns = new List<string>();

            foreach (var (fieldName, value) in model.Value.Fields)
            {
                var type = MapFieldType(value.Type);
                var column = fieldName == "id"
                    ? $"{Quote(fieldName)} {type} PRIMARY KEY"
                    : BuildColumnDef(fieldName, type, value);

                columns.Add(column);
            }

            sql += $"({string.Join(",\n", columns)});\n\n";
        }

        return sql;
    }

    private string BuildColumnDef(string fieldName, string type, FieldSchema value)
    {
        var column = $"{Quote(fieldName)} {type} ";
        if (value.Required) column += "NOT NULL ";
        if (value.Unique) column += "UNIQUE ";
        if (value.DefaultValue != null) column += $"DEFAULT {value.DefaultValue} ";
        if (value.References != null) column += $"REFERENCES {Quote(value.References)}({Quote("id")})";
        return column;
    }
    
    private Dictionary<string, object?>? ToDictionary(dynamic? result)
    {
        return result is null 
            ? null 
            : ((IDictionary<string, object?>)result).ToDictionary(x => x.Key, x => x.Value);
    }
}