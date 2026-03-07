using BetterAuth.Core;
using BetterAuth.Plugins;

namespace BetterAuth.Abstractions;

public interface IBetterAuthPlugin
{
    string Id { get; }
    
    PluginSchema? Schema { get; }
    
    IReadOnlyList<AuthHook>? Hooks { get; }
    
    IReadOnlyList<AuthEndpointDefinition>? Endpoints { get; }
    
    void OnRegister(AuthContext context) {}
}