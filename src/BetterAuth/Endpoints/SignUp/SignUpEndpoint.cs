using BetterAuth.Abstractions;
using BetterAuth.Core;
using BetterAuth.Errors;
using BetterAuth.Models.Inputs;
using BetterAuth.Plugins;
using BetterAuth.Services;
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

            var user = await ctx.AuthContext.AuthService.CreateUserAsync(new()
            {
                Email = ctx.Body["Email"]!.ToString()!,
                Image = ctx.Body.GetValueOrDefault("Image")?.ToString(),
                Name = ctx.Body["Name"]!.ToString()!
            }, ctx.HttpContext.RequestServices);
            
            // for now email/password is only supported, accountId = userId

            var password = await ctx.AuthContext.PasswordHasher.HashAsync(ctx.Body["Password"]!.ToString()!);

            await ctx.AuthContext.InternalAdapter.CreateAccountAsync(new CreateAccountInput
            {
                AccountId = user.Id,
                Password = password,
                ProviderId = "credential",
                UserId = user.Id
            });

            var session = await ctx.AuthContext.AuthService.CreateSessionAsync(user.Id, new SessionMetadata()
            {
                UserAgent = ctx.Request.Headers.UserAgent.ToString(),
                IpAddress = ctx.Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString(),
            }, ctx.HttpContext.RequestServices);
            
            var cookieOptions = ctx.AuthContext.Options.Session.Cookie;

            cookieOptions.Expires = DateTime.UtcNow.Add(ctx.AuthContext.Options.Session.ExpiresIn);
            
            ctx.SetCookie(ctx.AuthContext.Options.SessionCookieName, session.Token, cookieOptions);

            if (!ctx.AuthContext.Options.EmailVerification.SendOnSignUp)
            {
                return ctx.Json(new Dictionary<string, object?>()
                {
                    ["token"] = session.Token,
                    ["user"] =  user
                });
            }

            var emailService = ctx.Resolve<EmailVerificationService>();
            _ = emailService.SendVerificationAsync(user, ctx.Body.GetValueOrDefault("CallbackUrl")?.ToString() ?? "", ctx.HttpContext);

            return ctx.Json(new Dictionary<string, object?>
            {
                ["token"] = session.Token,
                ["user"] = user,
            });
        }
    };
}