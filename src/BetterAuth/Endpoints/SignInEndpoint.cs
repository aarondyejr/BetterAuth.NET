using BetterAuth.Core;
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
            var user =
                await ctx.AuthContext.InternalAdapter.FindUserByEmailAsync(ctx.Body["email"]!.ToString()!, true);
            
            if (user == null) throw AuthApiException.BadRequest("Account not found.");

            var account = user.Accounts.Find(a => a.ProviderId == "credential");

            if (account == null) throw AuthApiException.BadRequest("Account has to be credential");

            var isPasswordCorrect =
                await ctx.AuthContext.PasswordHasher.VerifyAsync(account.Password!, ctx.Body["password"]!.ToString()!);

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