using BetterAuth.Core;
using BetterAuth.Errors;
using BetterAuth.Plugins;

namespace BetterAuth.Endpoints;

public class SignOutEndpoint : IAuthEndpoint
{
    public AuthEndpointDefinition Definition => new()
    {
        Path = "/sign-out",
        Method = HttpMethodType.POST,
        Handler = async ctx =>
        {
            var sessionCookie = ctx.Request.Cookies["better-auth.session_token"];

            if (string.IsNullOrEmpty(sessionCookie)) throw AuthApiException.Unauthorized();

            var session = await ctx.AuthContext.InternalAdapter.FindSessionByTokenAsync(sessionCookie);

            if (session == null) throw AuthApiException.Unauthorized();

            await ctx.AuthContext.InternalAdapter.DeleteSessionAsync(session.Token);

            ctx.Request.HttpContext.Response.Cookies.Delete("better-auth.session_token");

            return ctx.Json(new Dictionary<string, object>
            {
                ["success"] = true
            });
        }
    };
}