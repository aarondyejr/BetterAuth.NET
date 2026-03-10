using BetterAuth.Core;
using BetterAuth.Endpoints.SignIn;
using BetterAuth.Errors;
using BetterAuth.Models.Inputs;
using BetterAuth.Plugins;
using Microsoft.AspNetCore.Http;

namespace BetterAuth.Endpoints;

internal class SignInEndpoint : IAuthEndpoint
{
    public AuthEndpointDefinition Definition => new()
    {
        Path = "/sign-in/email",
        Method = HttpMethodType.POST,
        Validator = _ => new SignInValidator(),
        Handler = async ctx =>
        {
            foreach (var (key, value) in ctx.Body)
            {
                Console.WriteLine($"{key}: {value}");
            }
            var user =
                await ctx.AuthContext.InternalAdapter.FindUserByEmailAsync(ctx.Body["Email"]!.ToString()!, true);
            
            if (user == null) throw AuthApiException.BadRequest("Account not found.");

            var account = user.Accounts.Find(a => a.ProviderId == "credential");

            if (account == null) throw AuthApiException.BadRequest("Account has to be credential");

            var isPasswordCorrect =
                await ctx.AuthContext.PasswordHasher.VerifyAsync(account.Password!, ctx.Body["Password"]!.ToString()!);

            if (!isPasswordCorrect) throw AuthApiException.BadRequest("Incorrect password");
            
            var session = await ctx.AuthContext.InternalAdapter.CreateSessionAsync(user.Id, new SessionMetadata
            {
                UserAgent = ctx.Request.Headers["User-Agent"].ToString(),
                IpAddress = ctx.Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString(),
            });
            
            ctx.SetCookie("better-auth.session_token", session.Token, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = true,
                Expires = session.ExpiresAt
            });

            return ctx.Json(new Dictionary<string, object?>
            {
                ["token"] = session.Token,
                ["user"] = new
                {
                    user.Id,
                    user.Email,
                    user.Name,
                    user.Image,
                    user.EmailVerified,
                    user.CreatedAt,
                    user.UpdatedAt,
                },
            });
        }
    };
}