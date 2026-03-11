using BetterAuth.Plugins;

namespace BetterAuth.Abstractions;

internal interface IAuthEndpoint
{
    AuthEndpointDefinition Definition { get; }
}