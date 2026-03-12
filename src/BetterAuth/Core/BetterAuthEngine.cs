using System.Reflection;
using BetterAuth.Abstractions;
using BetterAuth.Configuration;
using BetterAuth.Crypto;
using BetterAuth.Events;
using BetterAuth.Plugins;
using BetterAuth.Services;
using Microsoft.AspNetCore.Routing;

namespace BetterAuth.Core;

public class BetterAuthEngine
{
    public BetterAuthOptions Options { get; }
    public IInternalAdapter InternalAdapter { get; }
    public IPasswordHasher PasswordHasher { get; }
    public PluginRegistry PluginRegistry { get; }
    
    public string Secret => ResolveSecret(Options);
    
    public EventBus EventBus { get; }
    
    public AuthService AuthService { get; }

    public BetterAuthEngine(BetterAuthOptions options, EventBus eventBus)
    {
        EventBus = eventBus;
        
        Options = options;
        
        var adapter = options.DatabaseAdapter;
        
        InternalAdapter = new InternalAdapter(adapter, options);

        PasswordHasher = new BCryptPasswordHasher();
        
        PluginRegistry = new PluginRegistry();

        AuthService = new AuthService(InternalAdapter, EventBus);

        var context = new AuthContext
        {
            Options = options,
            InternalAdapter = InternalAdapter,
            PasswordHasher = PasswordHasher,
            Secret = Secret,
            DatabaseAdapter = options.DatabaseAdapter,
            AuthService = AuthService
        };

        foreach (var plugin in options.Plugins)
        {
            PluginRegistry.Register(plugin, context);
        }
    }

    public void MapRoutes(IEndpointRouteBuilder app)
    {
        app.MapBetterAuth(this);
    }

    public IEnumerable<AuthEndpointDefinition> GetCoreEndpoints() => Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => typeof(IAuthEndpoint).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
        .Select(t => Activator.CreateInstance(t) as IAuthEndpoint)
        .Where(e => e != null)
        .Select(e => e!.Definition);

    
    private static string ResolveSecret(BetterAuthOptions options)
    {
        if (!string.IsNullOrEmpty(options.Secret))
            return options.Secret;

        var envSecret = Environment.GetEnvironmentVariable("BETTER_AUTH_SECRET");
        if (!string.IsNullOrEmpty(envSecret))
            return envSecret;

        throw new InvalidOperationException(
            "Secret is not configured. Set it in BetterAuthOptions or the BETTER_AUTH_SECRET environment variable.");
    }

    public async Task MigrateAsync()
    {
        var schema = GetBaseSchema();

        foreach (var pluginSchema in PluginRegistry.GetAllSchemas())
        {
            MergeSchema(schema, pluginSchema);
        }

        var sql = Options.DatabaseAdapter.GenerateMigrationSql(schema);

        await Options.DatabaseAdapter.ExecuteMigrationAsync(sql);
    }
    
    private void MergeSchema(PluginSchema baseSchema, PluginSchema pluginSchema)
    {
        foreach (var (modelName, modelSchema) in pluginSchema.Models)
        {
            if (baseSchema.Models.TryGetValue(modelName, out var model))
            {
                foreach (var (fieldName, fieldSchema) in modelSchema.Fields)
                {
                    model.Fields[fieldName] = fieldSchema;
                }
            }
            else
            {
                baseSchema.Models[modelName] = modelSchema;
            }
        }
    }

    private PluginSchema GetBaseSchema()
    {
        return new PluginSchema
        {
            Models = new()
            {
                ["user"] = new ModelSchema
                {
                    Fields = new()
                    {
                        ["id"] = new FieldSchema { Type = FieldType.String, Required = true, Unique = true },
                        ["email"] = new FieldSchema { Type = FieldType.String, Required = true, Unique = true },
                        ["emailVerified"] = new FieldSchema { Type = FieldType.Boolean, Required = true },
                        ["name"] = new FieldSchema { Type = FieldType.String, Required = true },
                        ["image"] = new FieldSchema { Type = FieldType.String },
                        ["createdAt"] = new FieldSchema { Type = FieldType.Date, Required = true },
                        ["updatedAt"] = new FieldSchema { Type = FieldType.Date, Required = true },
                    }
                },
                ["session"] = new ModelSchema
                {
                    Fields = new()
                    {
                        ["id"] = new FieldSchema { Type = FieldType.String, Required = true, Unique = true },
                        ["userId"] = new FieldSchema { Type = FieldType.String, Required = true, References = "user" },
                        ["token"] = new FieldSchema { Type = FieldType.String, Required = true, Unique = true },
                        ["expiresAt"] = new FieldSchema { Type = FieldType.Date, Required = true },
                        ["ipAddress"] = new FieldSchema { Type = FieldType.String },
                        ["userAgent"] = new FieldSchema { Type = FieldType.String },
                        ["createdAt"] = new FieldSchema { Type = FieldType.Date, Required = true },
                        ["updatedAt"] = new FieldSchema { Type = FieldType.Date, Required = true },
                    }
                },
                ["account"] = new ModelSchema
                {
                    Fields = new()
                    {
                        ["id"] = new FieldSchema { Type = FieldType.String, Required = true, Unique = true },
                        ["userId"] = new FieldSchema { Type = FieldType.String, Required = true, References = "user" },
                        ["providerId"] = new FieldSchema { Type = FieldType.String, Required = true },
                        ["accountId"] = new FieldSchema { Type = FieldType.String, Required = true },
                        ["password"] = new FieldSchema { Type = FieldType.String },
                        ["createdAt"] = new FieldSchema { Type = FieldType.Date, Required = true },
                        ["updatedAt"] = new FieldSchema { Type = FieldType.Date, Required = true },
                    }
                },
                ["verification"] = new ModelSchema
                {
                    Fields = new()
                    {
                        ["id"] = new FieldSchema { Type = FieldType.String, Required = true, Unique = true },
                        ["identifier"] = new FieldSchema { Type = FieldType.String, Required = true },
                        ["value"] = new FieldSchema { Type = FieldType.String, Required = true },
                        ["expiresAt"] = new FieldSchema { Type = FieldType.Date, Required = true },
                        ["createdAt"] = new FieldSchema { Type = FieldType.Date, Required = true },
                        ["updatedAt"] = new FieldSchema { Type = FieldType.Date, Required = true },
                    }
                }
            }
        };
    }
}