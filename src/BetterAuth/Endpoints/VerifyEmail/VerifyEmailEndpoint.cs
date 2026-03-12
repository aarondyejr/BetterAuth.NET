using BetterAuth.Abstractions;
using BetterAuth.Models.Inputs;
using BetterAuth.Plugins;

namespace BetterAuth.Endpoints.VerifyEmail;

public class VerifyEmailEndpoint : IAuthEndpoint
{
    public AuthEndpointDefinition Definition => new()
    {
        Path = "/verify-email",
        Method = HttpMethodType.GET,
        Handler = async ctx =>
        {
            var token = ctx.Query.GetValueOrDefault("token");
            var callbackUrl = ctx.Query.GetValueOrDefault("callbackURL") ?? ctx.BaseUrl;

            if (string.IsNullOrEmpty(token))
            {
                ctx.HttpContext.Response.Redirect($"{callbackUrl}?error=invalid_token");
                return null;
            }

            var record = await ctx.AuthContext.InternalAdapter.FindVerificationValueAsync(token);

            if (record is null || record.ExpiresAt < DateTime.UtcNow)
            {
                ctx.HttpContext.Response.Redirect($"{callbackUrl}?error=invalid_token");
                return null;
            }

            var user = await ctx.AuthContext.InternalAdapter.FindUserByEmailAsync(record.Identifier);
            
            if (user is null)
            {
                ctx.HttpContext.Response.Redirect($"{callbackUrl}?error=invalid_token");
                return null;
            }

            await ctx.AuthContext.InternalAdapter.UpdateUserAsync(user.Id, new Dictionary<string, object?>
            {
                ["emailVerified"] = true
            });
            
            await ctx.AuthContext.InternalAdapter.DeleteVerificationByIdentifierAsync(token);

            if (ctx.AuthContext.Options.EmailVerification.AutoSignInAfterVerification)
            {
                var session = await ctx.AuthContext.AuthService.CreateSessionAsync(user.Id, new SessionMetadata
                {
                    UserAgent = ctx.HttpContext.Request.Headers["User-Agent"],
                    IpAddress = ctx.HttpContext.Connection.RemoteIpAddress?.ToString(),
                }, ctx.HttpContext.RequestServices);
                
                var cookieOptions = ctx.AuthContext.Options.Session.Cookie;

                cookieOptions.Expires = DateTime.UtcNow.Add(ctx.AuthContext.Options.Session.ExpiresIn);
                
                ctx.SetCookie(ctx.AuthContext.Options.SessionCookieName, session.Token, cookieOptions);
            }

            ctx.HttpContext.Response.Redirect(ctx.IsValidCallbackUrl(callbackUrl) ? callbackUrl : "/");
            return ctx.Json(new Dictionary<string, object?>()
            {
                ["status"] = true,
            });

        }
    };
}