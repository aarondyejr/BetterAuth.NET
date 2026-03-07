using BetterAuth.Configuration;
using BetterAuth.Core;
using BetterAuth.Postgres;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var engine = new BetterAuthEngine(new BetterAuthOptions
{
    Secret = "test-secret-for-local-dev-only",
    DatabaseAdapter = BetterAuthDatabase.Postgres("postgresql://neondb_owner:npg_2UXoBbt6mNJv@ep-autumn-breeze-a4c0y4fe-pooler.us-east-1.aws.neon.tech/neondb?sslmode=require&channel_binding=require"),
});

await engine.MigrateAsync();

engine.MapRoutes(app);

app.Run();
