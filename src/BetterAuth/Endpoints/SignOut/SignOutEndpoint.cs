using BetterAuth.Abstractions;
using BetterAuth.Core;
using BetterAuth.Errors;
using BetterAuth.Events;
using BetterAuth.Events.Auth;
using BetterAuth.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace BetterAuth.Endpoints.SignOut;

public class SignOutEndpoint : IAuthEndpoint
{
    public AuthEndpointDefinition Definition => new()
    {
        Path = "/sign-out",
        Method = HttpMethodType.POST,
        Handler = async ctx =>
        {
            var sessionCookie = ctx.GetCookie("better-auth.session_token");

            if (string.IsNullOrEmpty(sessionCookie)) throw AuthApiException.Unauthorized();

            var session = await ctx.AuthContext.InternalAdapter.FindSessionByTokenAsync(sessionCookie);

            if (session == null) throw AuthApiException.Unauthorized();
            
            var user = await ctx.AuthContext.InternalAdapter.FindUserByIdAsync(session.UserId);
            
            if (user == null) throw AuthApiException.BadRequest("Account not found.");
            
            await ctx.AuthContext.AuthService.DeleteSessionAsync(session.Token, user, ctx.HttpContext.RequestServices);
            
            ctx.DeleteCookie("better-auth.session_token");

            return ctx.Json(new Dictionary<string, object>
            {
                ["success"] = true
            });
        }
    };
}