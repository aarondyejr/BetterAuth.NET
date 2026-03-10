using BetterAuth.Configuration;
using FluentValidation;

namespace BetterAuth.Plugins;

public class AuthEndpointDefinition
{
    public required string Path { get; init; }

    public required HttpMethodType Method { get; init; }
    
    public AuthRequestSchema? QuerySchema { get; init; }
    
    public Func<BetterAuthOptions, IValidator>? Validator { get; init; }

    public required Func<AuthEndpointContext, Task<object>> Handler { get; init; }
}

public enum HttpMethodType
{
    GET,
    POST,
    PUT,
    PATCH,
    DELETE
}