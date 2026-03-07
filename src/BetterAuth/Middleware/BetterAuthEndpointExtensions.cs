using BetterAuth.Configuration;
using BetterAuth.Core;
using BetterAuth.Errors;
using BetterAuth.Models;
using BetterAuth.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BetterAuth.Middleware;

public static class BetterAuthEndpointExtensions
{
    public static void MapBetterAuth(this IEndpointRouteBuilder app, BetterAuthEngine engine)
    {
        var basePath = engine.Options.BasePath ?? "/api/auth";

        foreach (var endpoint in engine.GetCoreEndpoints())
        {
            MapEndpoint(app, basePath, endpoint, engine);
        }

        foreach (var endpoint in engine.PluginRegistry.GetAllEndpoints())
        {
            MapEndpoint(app, basePath, endpoint, engine);
        }
    }
    
    private static string ResolveBaseUrl(BetterAuthOptions options, HttpRequest request)
    {
        if (!string.IsNullOrEmpty(options.BaseUrl))
            return options.BaseUrl;

        var envUrl = Environment.GetEnvironmentVariable("BETTER_AUTH_URL");
        if (!string.IsNullOrEmpty(envUrl))
            return envUrl;

        return $"{request.Scheme}://{request.Host}";
    }

    private static void MapEndpoint(
        IEndpointRouteBuilder app,
        string basePath,
        AuthEndpointDefinition endpoint,
        BetterAuthEngine engine)
    {
        var fullPath = $"{basePath}{endpoint.Path}";

        async Task<IResult> HandleRequest(HttpContext httpContext)
        {
            try
            {
                var logger = httpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger($"BetterAuth.Endpoint[{endpoint.Method} {endpoint.Path}]");
                
                var body = endpoint.Method != HttpMethodType.GET
                    ? await httpContext.Request.ReadFromJsonAsync<Dictionary<string, object?>>()
                    : null;

                if (endpoint.BodySchema != null)
                {
                    ValidateBody(body, endpoint.BodySchema);
                }

                var ctx = new AuthEndpointContext
                {
                    Request = httpContext.Request,
                    AuthContext = new()
                    {
                        Options = engine.Options,
                        InternalAdapter = engine.InternalAdapter,
                        DatabaseAdapter = engine.Options.DatabaseAdapter,
                        PasswordHasher = engine.PasswordHasher,
                        Logger = logger,
                        Secret = engine.Secret,
                    },
                    Body = body ?? new(),
                    Path = endpoint.Path,
                    BaseUrl = ResolveBaseUrl(engine.Options, httpContext.Request),
                    Session = httpContext.Items["BetterAuth.Session"] as SessionRecord,
                    User = httpContext.Items["BetterAuth.User"] as UserRecord,
                };

                foreach (var hook in engine.PluginRegistry.GetHooks(HookTiming.Before))
                {
                    if (hook.Matcher(ctx))
                    {
                        var result = await hook.Handler(ctx);
                        if (result.Response != null)
                            return Results.Json(result.Response);
                        ctx = result.Context;
                    }
                }

                var response = await endpoint.Handler(ctx);

                foreach (var hook in engine.PluginRegistry.GetHooks(HookTiming.After))
                {
                    if (hook.Matcher(ctx))
                        await hook.Handler(ctx);
                }

                return Results.Json(response);
            }
            catch (AuthApiException ex)
            {
                return Results.Json(
                    new { error = ex.Message },
                    statusCode: ex.StatusCode
                );
            }
        }

        _ = endpoint.Method switch
        {
            HttpMethodType.GET => app.MapGet(fullPath, (Delegate)HandleRequest),
            HttpMethodType.POST => app.MapPost(fullPath, (Delegate)HandleRequest),
            HttpMethodType.PUT => app.MapPut(fullPath, (Delegate)HandleRequest),
            HttpMethodType.PATCH => app.MapPatch(fullPath, (Delegate)HandleRequest),
            HttpMethodType.DELETE => app.MapDelete(fullPath, (Delegate)HandleRequest),
            _ => throw new ArgumentException($"Unsupported method: {endpoint.Method}")
        };
    }

    private static void ValidateBody(Dictionary<string, object?>? body, AuthRequestSchema schema)
    {
        foreach (var (field, validation) in schema.Fields)
        {
            if (validation.Required && (body == null || !body.ContainsKey(field) || body[field] == null))
            {
                throw AuthApiException.BadRequest($"'{field}' is required.");
            }
        }
    }
}