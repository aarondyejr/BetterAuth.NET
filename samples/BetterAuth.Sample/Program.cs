using BetterAuth.Configuration;
using BetterAuth.Core;
using BetterAuth.Events;
using BetterAuth.Postgres;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();

builder.Services.AddBetterAuth(new BetterAuthOptions
{
    Secret = "test-secret-for-local-dev-only",
    DatabaseAdapter = BetterAuthDatabase.Sqlite("Data Source=mydatabase.db"),
    Events = new AuthEventOptions
    {
        OnUserCreated = (evt, _) =>
        {
            Console.WriteLine($"User {evt.User.Name} was created at {evt.OccurredAt}");

            return Task.CompletedTask;
        }
    },
    EmailVerification = new()
    {
        SendVerificationEmail = (evt, _) =>
        {
            Console.WriteLine(evt.Url);
            
            return Task.CompletedTask;
        },
    }
});

var app = builder.Build();

app.UseBetterAuth();

app.Run();
