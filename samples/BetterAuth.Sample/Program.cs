using BetterAuth.Configuration;
using BetterAuth.Core;
using BetterAuth.Postgres;
using BetterAuth.Plugins;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();

builder.Services.AddBetterAuth(new BetterAuthOptions
{
    Secret = "test-secret-for-local-dev-only",
    DatabaseAdapter = BetterAuthDatabase.Sqlite("Data Source=mydatabase.db"),
    Plugins = [new TestPlugin(new() { Id = "test-plugin-id" })]
});

var app = builder.Build();

app.UseBetterAuth();

await app.MigrateBetterAuthAsync();

app.Run();
