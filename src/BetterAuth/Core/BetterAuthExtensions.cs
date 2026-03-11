using System.Text.RegularExpressions;
using BetterAuth.Authorization;
using BetterAuth.Configuration;
using BetterAuth.Events;
using BetterAuth.Events.Auth;
using BetterAuth.Middleware;
using BetterAuth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BetterAuth.Core;

public static class BetterAuthExtensions
{
    public static IServiceCollection AddBetterAuth(this IServiceCollection services, BetterAuthOptions options)
    {
        
        if (options.DatabaseAdapter == null)
            throw new InvalidOperationException("BetterAuthOptions.DatabaseAdapter is required.");

        services.AddCors(cors => cors.AddPolicy("BetterAuth", policy =>
        {
            var compiledPatterns = (options.TrustedOrigins ?? [])
                .Where(o => o.Contains('*'))
                .Select(o => new Regex(
                    "^" + Regex.Escape(o).Replace("\\*", ".*") + "$",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled))
                .ToList();

            var exactOrigins = (options.TrustedOrigins ?? [])
                .Where(o => !o.Contains('*'))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            policy
                .SetIsOriginAllowed(origin =>
                    exactOrigins.Contains(origin) ||
                    compiledPatterns.Any(p => p.IsMatch(origin)))
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }));
        
        services.AddAuthorizationBuilder()
            .AddPolicy("BetterAuth", policy => policy.AddRequirements(new BetterAuthRequirement()));
        
        var eventBus = new EventBus();
        var engine = new BetterAuthEngine(options, eventBus);
        
        services.AddSingleton(engine);
        services.AddScoped<EmailVerificationService>();
        services.AddScoped<IAuthorizationHandler, BetterAuthHandler>();
        services.AddHttpContextAccessor();
        services.AddSingleton<IEventBus>(eventBus);

        options.Events?.RegisterAll(eventBus);
        
        return services;
    }

    extension(WebApplication app)
    {
        public WebApplication UseBetterAuth()
        {
            var engine = app.Services.GetRequiredService<BetterAuthEngine>();
            app.UseCors("BetterAuth");
        
            app.UseMiddleware<BetterAuthSessionMiddleware>(engine);
        
            engine.MapRoutes(app);

            return app;
        }

        public async Task<WebApplication> MigrateBetterAuthAsync()
        {
            var engine = app.Services.GetRequiredService<BetterAuthEngine>();

            await engine.MigrateAsync();

            return app;
        }
    }
}