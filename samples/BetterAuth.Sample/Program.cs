using BetterAuth.Configuration;
using BetterAuth.Core;
using BetterAuth.Postgres;
using BetterAuth.Plugins;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
var app = builder.Build();

var engine = new BetterAuthEngine(new BetterAuthOptions
{
    Secret = "test-secret-for-local-dev-only",
    DatabaseAdapter = BetterAuthDatabase.Postgres("postgresql://neondb_owner:npg_Cr9PNBlHy2VZ@ep-autumn-breeze-a4c0y4fe-pooler.us-east-1.aws.neon.tech/neondb?sslmode=require&channel_binding=require"),
    Plugins = [new TestPlugin(new() { Id = "test-plugin-id" })]
});

await engine.MigrateAsync();

app.UseCors(policy => policy
    .WithOrigins("http://localhost:3000")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials());

engine.MapRoutes(app);


app.Run();
