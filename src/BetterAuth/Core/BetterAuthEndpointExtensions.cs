using System.Text.Json;
using BetterAuth.Configuration;
using BetterAuth.Core;
using BetterAuth.Errors;
using BetterAuth.Models;
using BetterAuth.Plugins;
using BetterAuth.Services;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BetterAuth.Core;

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
        var cachedValidator = endpoint.Validator?.Invoke(engine.Options);
        Type? requestType = cachedValidator != null ? GetValidatorRequestType(cachedValidator) : null;
        
        async Task<IResult> HandleRequest(HttpContext httpContext)
        {
            try
            {
                
                var logger = httpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger($"BetterAuth.Endpoint[{endpoint.Method} {endpoint.Path}]");

                Dictionary<string, object?>? body = null;

                if (endpoint.Method != HttpMethodType.GET && httpContext.Request.ContentLength > 0)
                {

                    if (cachedValidator != null)
                    {
                        var typedBody = await httpContext.Request.ReadFromJsonAsync(requestType!);
                        
                        var result = await cachedValidator.ValidateAsync(new ValidationContext<object>(typedBody!));
                        
                        if (!result.IsValid) 
                            throw AuthApiException.BadRequest(result.Errors.Select(e => new ValidationError
                            {
                                Field = e.PropertyName.ToLower(),
                                Message = e.ErrorMessage,
                            }).ToList());
                        
                        body = JsonSerializer.Deserialize<Dictionary<string, object?>>(JsonSerializer.Serialize(typedBody));
                    }
                    else
                    {
                        body = await httpContext.Request.ReadFromJsonAsync<Dictionary<string, object?>>();
                    }
                }

                var ctx = new AuthEndpointContext
                {
                    HttpContext = httpContext,
                    AuthContext = new()
                    {
                        Options = engine.Options,
                        InternalAdapter = engine.InternalAdapter,
                        DatabaseAdapter = engine.Options.DatabaseAdapter,
                        PasswordHasher = engine.PasswordHasher,
                        Logger = logger,
                        Secret = engine.Secret,
                        AuthService = new AuthService(engine.InternalAdapter, engine.EventBus)
                    },
                    Body = body ?? new(),
                    Path = endpoint.Path,
                    BaseUrl = ResolveBaseUrl(engine.Options, httpContext.Request),
                    Session = httpContext.Items["BetterAuth.Session"] as SessionRecord,
                    User = httpContext.Items["BetterAuth.User"] as UserRecord,
                    Query = httpContext.Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString().Trim())
                };

                foreach (var hook in engine.PluginRegistry.GetHooks(HookTiming.Before))
                {
                    if (!hook.Matcher(ctx)) continue;
                    
                    var result = await hook.Handler(ctx);
                    if (result.Response != null)
                        return Results.Json(result.Response);
                    ctx = result.Context;
                }

                var response = await endpoint.Handler(ctx);

                if (response is null)
                    return Results.Empty;

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
                    new { error = ex.Payload },
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
    
    private static Type GetValidatorRequestType(IValidator validator)
    {
        var type = validator.GetType();
    
        while (type != null)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
                return type.GetGenericArguments()[0];
        
            type = type.BaseType;
        }
    
        throw new InvalidOperationException($"Could not determine request type for validator {validator.GetType().Name}");
    }
}