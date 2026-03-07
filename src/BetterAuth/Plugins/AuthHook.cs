namespace BetterAuth.Plugins;

public class AuthHook
{
    /// When this hook runs relative to the endpoint handler
    public required HookTiming Timing { get; init; }

    /// Determines which requests this hook applies to
    public required Func<AuthEndpointContext, bool> Matcher { get; init; }

    /// The hook logic. Can modify context, throw to reject, or pass through.
    public required Func<AuthEndpointContext, Task<HookResult>> Handler { get; init; }
}

public enum HookTiming
{
    Before,
    After
}

public class HookResult
{
    /// The (possibly modified) context to pass along
    public AuthEndpointContext Context { get; init; }

    /// If set, short-circuits and returns this response immediately
    public object? Response { get; init; }
}