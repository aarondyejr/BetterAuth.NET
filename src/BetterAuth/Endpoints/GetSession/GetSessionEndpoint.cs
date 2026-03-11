using BetterAuth.Abstractions;
using BetterAuth.Core;
using BetterAuth.Errors;
using BetterAuth.Plugins;

namespace BetterAuth.Endpoints;

public class GetSessionEndpoint : IAuthEndpoint
{
    public AuthEndpointDefinition Definition => new()
    {
        Path = "/get-session",
        Method = HttpMethodType.GET,
        Handler = async ctx =>
        {
            var sessionCookie = ctx.Request.Cookies["better-auth.session_token"];
            
            if (string.IsNullOrEmpty(sessionCookie)) throw AuthApiException.Unauthorized();

            var session = await ctx.AuthContext.InternalAdapter.FindSessionByTokenAsync(sessionCookie);
            
            if (session == null) throw AuthApiException.Unauthorized();
            
            if (session.ExpiresAt < DateTime.UtcNow) throw AuthApiException.Forbidden("Expired Session");

            var user = await ctx.AuthContext.InternalAdapter.FindUserByIdAsync(session.UserId);
            
            if (user == null) throw AuthApiException.Unauthorized();

            return ctx.Json(new Dictionary<string, object?>
            {
                ["session"] = session,
                ["user"] = user,
            });
        }
    };
}