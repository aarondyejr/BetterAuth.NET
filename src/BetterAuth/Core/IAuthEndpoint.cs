using BetterAuth.Plugins;

namespace BetterAuth.Core;

internal interface IAuthEndpoint
{
    AuthEndpointDefinition Definition { get; }
}