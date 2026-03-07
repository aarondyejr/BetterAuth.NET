namespace BetterAuth.Plugins;

public class AuthEndpointDefinition
{
    /// Route path (e.g., "/email-otp/send-verification-otp")
    public required string Path { get; init; }

    /// HTTP method
    public required HttpMethodType Method { get; init; }

    /// Request body schema for validation (null for GET)
    public AuthRequestSchema? BodySchema { get; init; }

    /// Query parameter schema for validation
    public AuthRequestSchema? QuerySchema { get; init; }

    /// Whether this endpoint requires an authenticated session
    public bool RequiresAuth { get; init; } = false;

    /// The endpoint handler
    public required Func<AuthEndpointContext, Task<object>> Handler { get; init; }

    // Optional metadata (for OpenAPI generation, etc.)
    // public EndpointMetadata? Metadata { get; init; }
}

public enum HttpMethodType
{
    GET,
    POST,
    PUT,
    PATCH,
    DELETE
}