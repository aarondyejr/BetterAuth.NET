using BetterAuth.Adapters.Args;
using BetterAuth.Plugins;

namespace BetterAuth.Abstractions;

public interface IAuthDatabaseAdapter
{
    Task<Dictionary<string, object?>> CreateAsync(CreateArgs args);
    
    Task<Dictionary<string, object?>> UpdateAsync(UpdateArgs args);
    
    Task<int> UpdateManyAsync(UpdateManyArgs args);
    
    Task<bool> DeleteAsync(DeleteArgs args);
    
    Task<int> DeleteManyAsync(DeleteManyArgs args);
    
    Task<Dictionary<string, object?>?> FindOneAsync(FindOneArgs args);
    
    Task<List<Dictionary<string, object?>>> FindManyAsync(FindManyArgs args);
    
    Task<int> CountAsync(CountArgs args);
    
    Task<T> TransactionAsync<T>(Func<IAuthDatabaseAdapter, Task<T>> operation);
    Task TransactionAsync(Func<IAuthDatabaseAdapter, Task> operation);
    
    string GenerateMigrationSql(PluginSchema schema);
    
    Task ExecuteMigrationAsync(string sql);
}