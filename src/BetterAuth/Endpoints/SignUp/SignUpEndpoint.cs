using BetterAuth.Core;
using BetterAuth.Errors;
using BetterAuth.Models.Inputs;
using BetterAuth.Plugins;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BetterAuth.Endpoints.SignUp;

public class SignUpEndpoint : IAuthEndpoint
{
    public AuthEndpointDefinition Definition => new()
    {
        Path = "/sign-up/email",
        Method = HttpMethodType.POST,
        Validator = options => new SignUpValidator(options.EmailAndPassword),
        Handler = async ctx =>
        {
            var exists = await ctx.AuthContext.InternalAdapter.FindUserByEmailAsync(ctx.Body["Email"]!.ToString()!, false);

            if (exists != null) throw AuthApiException.BadRequest("Account already exists.");

            var user = await ctx.AuthContext.InternalAdapter.CreateUserAsync(new()
            {
                Email = ctx.Body["Email"]!.ToString()!,
                Image = ctx.Body.GetValueOrDefault("Image")?.ToString(),
                Name = ctx.Body["Name"]!.ToString()!
            });
            // for now email/password is only supported, accountIds = userId

            var password = await ctx.AuthContext.PasswordHasher.HashAsync(ctx.Body["Password"]!.ToString()!);

            await ctx.AuthContext.InternalAdapter.CreateAccountAsync(new()
            {
                AccountId = user.Id,
                Password = password,
                ProviderId = "credential",
                UserId = user.Id
            });

            var session = await ctx.AuthContext.InternalAdapter.CreateSessionAsync(user.Id, new SessionMetadata()
            {
                UserAgent = ctx.Request.Headers["User-Agent"].ToString(),
                IpAddress = ctx.Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString(),
            });
            
            ctx.SetCookie("better-auth.session_token", session.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = session.ExpiresAt
            });

            return ctx.Json(new Dictionary<string, object?>
            {
                ["token"] = session.Token,
                ["user"] = user,
            });
        }
    };
}