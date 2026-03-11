using BetterAuth.Core;
using BetterAuth.Models.Inputs;
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

                if (session.ExpiresAt - DateTime.UtcNow < engine.Options.Session.UpdateAge)
                {
                    var metadata = new SessionMetadata
                    {
                        UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
                        IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
                    };

                    var refreshed = await engine.InternalAdapter.RefreshSessionAsync(session, metadata);

                    if (refreshed != null)
                    {
                        var cookieOptions = engine.Options.Session.Cookie;

                        cookieOptions.Expires = DateTime.UtcNow.Add(engine.Options.Session.ExpiresIn);
                        
                        session = refreshed;
                        httpContext.Response.Cookies.Append("better-auth.session_token", session.Token, cookieOptions);
                    }
                }


                httpContext.Items["BetterAuth.Session"] = session;
                var user = await engine.InternalAdapter.FindUserByIdAsync(session.UserId);
                httpContext.Items["BetterAuth.User"] = user;
            }
        }

        await next(httpContext);
    }
}