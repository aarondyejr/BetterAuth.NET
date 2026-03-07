using BetterAuth.Core;
using Microsoft.AspNetCore.Http;

namespace BetterAuth.Middleware;

public class BetterAuthSessionMiddleware(RequestDelegate next, BetterAuthEngine engine)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        var sessionToken = httpContext.Request.Cookies["better-auth.session_token"];

        if (!string.IsNullOrEmpty(sessionToken))
        {
            var session = await engine.InternalAdapter.FindSessionByTokenAsync(sessionToken);

            if (session != null && session.ExpiresAt > DateTime.UtcNow)
            {
                // Attach session and user to the request context
                httpContext.Items["BetterAuth.Session"] = session;
                var user = await engine.InternalAdapter.FindUserByIdAsync(session.UserId);
                httpContext.Items["BetterAuth.User"] = user;
            }
        }

        await next(httpContext);
    }
}