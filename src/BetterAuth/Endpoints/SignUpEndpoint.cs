using BetterAuth.Core;
using BetterAuth.Errors;
using BetterAuth.Models.Inputs;
using BetterAuth.Plugins;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BetterAuth.Endpoints;

public class SignUpEndpoint : IAuthEndpoint
{
    public AuthEndpointDefinition Definition => new()
    {
        Path = "/sign-up/email",
        Method = HttpMethodType.POST,
        BodySchema = new AuthRequestSchema
        {
            Fields = new()
            {
                ["email"] = new FieldValidation
                {
                    Type = FieldType.String,
                    Required = true,
                    Description = "The user's email address",
                },
                ["password"] = new FieldValidation
                {
                    Type = FieldType.String,
                    Required = true,
                    Description = "The user's password",
                },
                ["name"] = new FieldValidation
                {
                    Type = FieldType.String,
                    Required = true,
                    Description = "The user's display name",
                },
                ["image"] = new FieldValidation
                {
                    Type = FieldType.String,
                    Required = false,
                    Description = "Profile image URL",
                },
                ["callbackURL"] = new FieldValidation
                {
                    Type = FieldType.String,
                    Required = false,
                    Description = "URL to redirect after email verification",
                },
            }
        },
        Handler = async ctx =>
        {
            var exists = await ctx.AuthContext.InternalAdapter.FindUserByEmailAsync(ctx.Body["email"]!.ToString()!, false);

            if (exists != null) throw AuthApiException.BadRequest("Account already exists.");

            var user = await ctx.AuthContext.InternalAdapter.CreateUserAsync(new()
            {
                Email = ctx.Body["email"]!.ToString()!,
                Image = ctx.Body.GetValueOrDefault("image")?.ToString(),
                Name = ctx.Body["name"]!.ToString()!
            });
            // for now email/password is only supported, accountIds = userId

            var password = await ctx.AuthContext.PasswordHasher.HashAsync(ctx.Body["password"]!.ToString()!);

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