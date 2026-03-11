using System.Diagnostics;
using System.Text.Json;
using BetterAuth.Abstractions;
using BetterAuth.Database.Args;
using BetterAuth.Plugins;

namespace BetterAuth.Database;

public class TransformingAdapter(
    AdapterConfig config,
    IAuthDatabaseAdapter rawAdapter) : IAuthDatabaseAdapter
{
    public async Task<Dictionary<string, object?>> CreateAsync(CreateArgs args)
    {
        var transformData = TransformInput(args.Data);
        var transformedModel = config.UsePlural ? Pluralize(args.Model) : args.Model;
        var newArgs = args with { Model = transformedModel, Data = transformData };

        var result = await rawAdapter.CreateAsync(newArgs);

        return TransformOutput(result);
    }

    public async Task<Dictionary<string, object?>> UpdateAsync(UpdateArgs args)
    {
        var transformData =  TransformInput(args.Data);
        var transformedModel = config.UsePlural ? Pluralize(args.Model) : args.Model;
        var transformedWhere = TransformWhereClauses(args.Where);
        var newArgs = new UpdateArgs { Model = transformedModel, Data = transformData, Where = transformedWhere };
        
        var result = await rawAdapter.UpdateAsync(newArgs);
        
        return TransformOutput(result);
    }

    public async Task<int> UpdateManyAsync(UpdateManyArgs args)
    {
        var transformData = TransformInput(args.Data);
        var transformedModel = config.UsePlural ? Pluralize(args.Model) : args.Model;
        var transformedWhere = TransformWhereClauses(args.Where);
        var newArgs = new UpdateManyArgs { Model = transformedModel, Data = transformData, Where = transformedWhere };
        
        return await rawAdapter.UpdateManyAsync(newArgs);
    }

    public async Task<bool> DeleteAsync(DeleteArgs args)
    {
        var transformedModel = config.UsePlural ? Pluralize(args.Model) : args.Model;
        var transformedWhere = TransformWhereClauses(args.Where);
        var newArgs = new DeleteArgs { Model = transformedModel, Where = transformedWhere };
        
        return await rawAdapter.DeleteAsync(newArgs);
    }

    public async Task<int> DeleteManyAsync(DeleteManyArgs args)
    {
        var transformedModel = config.UsePlural ? Pluralize(args.Model) : args.Model;
        var transformedWhere = TransformWhereClauses(args.Where);
        var newArgs = new DeleteManyArgs { Model = transformedModel, Where = transformedWhere };
        
        return await rawAdapter.DeleteManyAsync(newArgs);
    }

    public async Task<Dictionary<string, object?>?> FindOneAsync(FindOneArgs args)
    {
        var transformedModel = config.UsePlural ? Pluralize(args.Model) : args.Model;
        var transformedSelect = TransformSelect(args.Select);
        var transformedWhere = TransformWhereClauses(args.Where);

        var newArgs = new FindOneArgs { Model = transformedModel, Where = transformedWhere, Select = transformedSelect };
        
        var result = await  rawAdapter.FindOneAsync(newArgs);

        return result != null ? TransformOutput(result) : null;
    }

    public async Task<List<Dictionary<string, object?>>> FindManyAsync(FindManyArgs args)
    {
        var transformedModel = config.UsePlural ? Pluralize(args.Model) : args.Model;
        var transformedSelect = TransformSelect(args.Select);
        var transformedWhere = TransformWhereClauses(args.Where ?? []);

        var newArgs = new FindManyArgs
        {
            Model = transformedModel,
            Where = transformedWhere,
            Select = transformedSelect,
            Limit = args.Limit,
            Offset = args.Offset,
            SortBy = args.SortBy != null 
                ? new SortBy
                {
                    Field = config.MapKeysInput?.GetValueOrDefault(args.SortBy.Field, args.SortBy.Field) ?? args.SortBy.Field,
                    Direction = args.SortBy.Direction
                } 
                : null
        };

        var results = await rawAdapter.FindManyAsync(newArgs);
        return results.Select(TransformOutput).ToList();
    }

    public async Task<int> CountAsync(CountArgs args)
    {
        var transformedModel = config.UsePlural ? Pluralize(args.Model) : args.Model;
        var transformedWhere = TransformWhereClauses(args.Where ?? []);
        
        var newArgs = new CountArgs
            { Model = transformedModel, Where = transformedWhere };

        return await rawAdapter.CountAsync(newArgs);
    }

    public async Task<T> TransactionAsync<T>(Func<IAuthDatabaseAdapter, Task<T>> operation)
    {
        return await rawAdapter.TransactionAsync(async adapter => await operation(this));
    }

    public async Task TransactionAsync(Func<IAuthDatabaseAdapter, Task> operation)
    {
        await rawAdapter.TransactionAsync(async adapter =>
        {
            await operation(this);
        });
    }

    public string GenerateMigrationSql(PluginSchema schema)
    {
        return rawAdapter.GenerateMigrationSql(schema);
    }

    public async Task ExecuteMigrationAsync(string sql)
    {
       await rawAdapter.ExecuteMigrationAsync(sql);
    }


    private List<WhereClause> TransformWhereClauses(List<WhereClause> clauses)
    {
        return clauses.Select(clause =>
        {
            var field = config.MapKeysInput?.GetValueOrDefault(clause.Field, clause.Field) ?? clause.Field;
            var value = TransformValue(clause.Value);
            return new WhereClause { Field = field, Operator = clause.Operator, Value = value };
        }).ToList();
    }
    
    private object? TransformValue(object? value)
    {
        if (!config.SupportsBooleans && value is bool b)
            return b ? 1 : 0;

        switch (config.SupportsDates)
        {
            case false when value is DateTime dt:
                return dt.ToUniversalTime().ToString("O");
            case true when value is DateTime dt2 && dt2.Kind != DateTimeKind.Utc:
                return dt2.ToUniversalTime();
        }

        if (!config.SupportsJson && value is not null && IsComplexType(value))
            return JsonSerializer.Serialize(value);

        return value;
    }

    private string[]? TransformSelect(string[]? values)
    {
        return values?.Select(v => config.MapKeysInput?.GetValueOrDefault(v, v) ?? v).ToArray();
    }
    
    private Dictionary<string, object?> TransformInput(Dictionary<string, object?> data)
    {
        var transformed = new Dictionary<string, object?>();

        foreach (var (key, value) in data)
        {
            var newKey = config.MapKeysInput?.GetValueOrDefault(key, key) ?? key;
            transformed[newKey] = TransformValue(value);
        }

        return transformed;
    }

    private Dictionary<string, object?> TransformOutput(Dictionary<string, object?> data)
    {
        var transformed = config.MapKeysOutput != null
            ? MapKeys(data, config.MapKeysOutput)
            : new Dictionary<string, object?>(data);

        foreach (var key in transformed.Keys.ToList())
        {
            var value = transformed[key];

            if (IsNumeric(value))
            {
                var longValue = Convert.ToInt64(value);
                if (longValue is 0 or 1)
                {
                    transformed[key] = longValue == 1;
                }
            }
        
            
            if (!config.SupportsDates && value is string s && DateTimeOffset.TryParse(s, out var dt))
            {
                transformed[key] = dt.UtcDateTime;
            }
            else if (value is DateTime dateTime && dateTime.Kind != DateTimeKind.Utc)
            {
                transformed[key] = dateTime.ToUniversalTime();
            }
        }

        return transformed;
    }

    private bool IsComplexType(object value)
    {
        return value is not (string or int or long or float or double or decimal or bool or DateTime);
    }

    private Dictionary<string, object?> MapKeys(Dictionary<string, object?> data, Dictionary<string, string> keyMap)
    {
        var result = new Dictionary<string, object?>();

        foreach (var (key, value) in data)
        {
            var newKey = keyMap.GetValueOrDefault(key, key);
            result[newKey] = value;
        }
        
        return result;
    }

    private string Pluralize(string model)
    {
        return $"{model}s";
    }
    
    private static bool IsNumeric(object? v) => 
        v is sbyte or byte or short or ushort or int or uint or long or ulong;
}