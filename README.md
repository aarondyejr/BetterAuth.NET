# BetterAuth.NET

Am unofficial C# port of [Better Auth](https://better-auth.com) — a framework-agnostic authentication library with a plugin system, built for ASP.NET Core.

> **Status:** Early development. Not ready for production use.

## What is this?

BetterAuth.NET brings the Better Auth experience to the .NET ecosystem. It's designed to be API-compatible with `better-auth/client`, meaning you can use a Next.js frontend with the standard Better Auth client and point it at a C# backend running BetterAuth.NET.

## Features (v0 scope)

- **Email/password authentication** — sign up, sign in, sign out, session management
- **Database-agnostic** — ships with a PostgreSQL adapter, bring your own for anything else
- **Plugin system** — extend the schema, hook into lifecycle events, add new endpoints
- **ASP.NET Core middleware** — session validation built into the request pipeline
- **API-compatible** — works with the existing `better-auth/client` without modification

## Quick Start

### Installation

```bash
dotnet add package BetterAuth
dotnet add package BetterAuth.Adapters
```

### Setup

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var engine = new BetterAuthEngine(new BetterAuthOptions
{
    Secret = "your-secret-key",   // or set BETTER_AUTH_SECRET env var
    BaseUrl = "http://localhost:5000",
    DatabaseAdapter = BetterAuthDatabase.Postgres("Host=localhost;Database=myapp;Username=postgres;Password=postgres"),
});

await engine.MigrateAsync(); // creates/updates tables

app.UseMiddleware<BetterAuthSessionMiddleware>(engine);
engine.MapRoutes(app);

app.Run();
```

### With plugins

```csharp
var engine = new BetterAuthEngine(new BetterAuthOptions
{
    Secret = "your-secret-key",
    BaseUrl = "http://localhost:5000",
    DatabaseAdapter = BetterAuthDatabase.Postgres("Host=localhost;Database=myapp;..."),
    Plugins = [ new MyCustomPlugin() ],
});
```

## Writing a Plugin

Plugins can extend the database schema, hook into auth lifecycle events, and register new endpoints.

```csharp
public class BirthdayPlugin : IBetterAuthPlugin
{
    public string Id => "birthdayPlugin";

    public PluginSchema Schema => new()
    {
        Models = new()
        {
            ["user"] = new ModelSchema
            {
                Fields = new()
                {
                    ["birthday"] = new FieldSchema
                    {
                        Type = FieldType.Date,
                        Required = true
                    }
                }
            }
        }
    };

    public IReadOnlyList<AuthHook> Hooks => new List<AuthHook>
    {
        new AuthHook
        {
            Timing = HookTiming.Before,
            Matcher = ctx => ctx.Path.StartsWith("/sign-up/email"),
            Handler = async ctx =>
            {
                var birthday = ctx.Body.GetValueOrDefault("birthday") as DateTime?;
                if (birthday == null)
                    throw new AuthApiException(400, "Birthday is required.");

                if (birthday >= DateTime.UtcNow.AddYears(-13))
                    throw new AuthApiException(400, "User must be at least 13 years old.");

                return new HookResult { Context = ctx };
            }
        }
    };

    public IReadOnlyList<AuthEndpointDefinition>? Endpoints => null;
}
```

## Writing a Database Adapter

Implement `IAuthDatabaseAdapter` to support any database. The adapter is a generic CRUD interface — it knows nothing about auth, just how to store and retrieve records.

```csharp
public class MyAdapter : IAuthDatabaseAdapter
{
    public async Task<Dictionary<string, object?>> CreateAsync(CreateArgs args) { /* ... */ }
    public async Task<Dictionary<string, object?>?> FindOneAsync(FindOneArgs args) { /* ... */ }
    public async Task<List<Dictionary<string, object?>>> FindManyAsync(FindManyArgs args) { /* ... */ }
    public async Task<Dictionary<string, object?>> UpdateAsync(UpdateArgs args) { /* ... */ }
    public async Task<int> UpdateManyAsync(UpdateManyArgs args) { /* ... */ }
    public async Task DeleteAsync(DeleteArgs args) { /* ... */ }
    public async Task<int> DeleteManyAsync(DeleteManyArgs args) { /* ... */ }
    public async Task<int> CountAsync(CountArgs args) { /* ... */ }
    public async Task<T> TransactionAsync<T>(Func<IAuthDatabaseAdapter, Task<T>> operation) { /* ... */ }
    public async Task TransactionAsync(Func<IAuthDatabaseAdapter, Task> operation) { /* ... */ }
    public string GenerateMigrationSql(PluginSchema schema) { /* ... */ }
    public async Task ExecuteMigrationAsync(string sql) { /* ... */ }
}
```

Wrap it in a `TransformingAdapter` if your database needs key mapping, boolean coercion, or date coercion:

```csharp
DatabaseAdapter = new TransformingAdapter(
    config: new AdapterConfig
    {
        UsePlural = true,           // "user" → "users"
        SupportsBooleans = false,   // coerce bool ↔ 0/1
        SupportsDates = false,      // coerce DateTime ↔ ISO string
        SupportsJson = false,       // coerce complex types to JSON string
        MapKeysInput = new() { ["id"] = "_id" },
        MapKeysOutput = new() { ["_id"] = "id" },
    },
    rawAdapter: new MyAdapter()
)
```

## Packages

| Package | Description |
|---------|-------------|
| `BetterAuth` | Core library — auth engine, plugin system, middleware |
| `BetterAuth.Adapters` | Database adapter implementations (PostgreSQL, ...) |

## Roadmap

- [x] Core architecture and plugin system
- [x] PostgreSQL adapter
- [x] Schema migrations (`MigrateAsync`)
- [x] Session middleware
- [ ] Email/password sign-in
- [ ] Email/password sign-up
- [ ] Sign-out / session management
- [ ] Email verification and password reset
- [ ] OAuth/social login providers
- [ ] Admin plugin
- [ ] Organization/multi-tenancy plugin
- [ ] CLI tool for schema generation

## Credits

This project is an unofficial C# port of [Better Auth](https://better-auth.com) by the Better Auth team. All credit for the original design, API surface, and plugin architecture goes to them.

## License

MIT