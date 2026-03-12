using BetterAuth.Adapters;
using BetterAuth.Configuration;
using BetterAuth.Core;
using BetterAuth.Events;
using BetterAuth.Providers;
using BetterAuth.Providers.Configuration;

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

builder.Services.AddBetterAuthStorage(new S3StorageOptions
{
    Endpoint = "s3.us-southeast-1.idrivee2.com",
    AccessKey = "O8Hru7NQExwqxJXkCKxL",
    SecretKey = "1ym4Sw1XwU99H47RvKCToGOJ22eaBVxalmqLCn2I",
    Bucket = "images",
});

var app = builder.Build();

app.UseBetterAuth();

await app.MigrateBetterAuthAsync();

app.Run();
