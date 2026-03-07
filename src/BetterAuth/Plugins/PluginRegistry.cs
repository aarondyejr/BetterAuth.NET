using BetterAuth.Abstractions;
using BetterAuth.Core;

namespace BetterAuth.Plugins;

public class PluginRegistry
{
    private readonly List<IBetterAuthPlugin> _plugins = new();

    public void Register(IBetterAuthPlugin plugin, AuthContext context)
    {
        _plugins.Add(plugin);
        plugin.OnRegister(context);
    }
    
    public IEnumerable<PluginSchema> GetAllSchemas() => _plugins.Where(p => p.Schema is not null).Select(p => p.Schema!);
    
    public IEnumerable<AuthHook> GetHooks(HookTiming timing) => _plugins.SelectMany(p => p.Hooks ?? Enumerable.Empty<AuthHook>()).Where(h => h.Timing == timing);
    
    public IEnumerable<AuthEndpointDefinition> GetAllEndpoints() => _plugins.SelectMany(p => p.Endpoints ?? Enumerable.Empty<AuthEndpointDefinition>());
}