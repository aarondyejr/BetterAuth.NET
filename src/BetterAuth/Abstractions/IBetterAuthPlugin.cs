using BetterAuth.Core;
using BetterAuth.Plugins;

namespace BetterAuth.Abstractions;

public interface IBetterAuthPlugin
{
    string Id { get; }
    PluginSchema? Schema => null;
    IReadOnlyList<AuthHook>? Hooks => null;
    IReadOnlyList<AuthEndpointDefinition>? Endpoints => null;
    void OnRegister(AuthContext context) { }
}