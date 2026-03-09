using BetterAuth.Abstractions;

namespace BetterAuth.Plugins;

public class Options
{
    public required string Id { get; init; }
}

public class TestPlugin : IBetterAuthPlugin
{
    
    Options? Options { get; }
    
    public TestPlugin()
    {
        
    }
    
    public TestPlugin(Options options)
    {
        Options = options;
    }
    
    public string Id => "TestPlugin";

    public IReadOnlyList<AuthEndpointDefinition>? Endpoints =>
    [
        new AuthEndpointDefinition
        {
            Path = "/two-factor",
            Method = HttpMethodType.POST,
            Handler = async ctx => ctx.Json(new Dictionary<string, object> { ["success"] = true })
        }
    ];
}