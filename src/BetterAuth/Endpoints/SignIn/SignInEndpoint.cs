using BetterAuth.Abstractions;
using BetterAuth.Errors;
using BetterAuth.Models.Inputs;
using BetterAuth.Plugins;
using BetterAuth.Services;

namespace BetterAuth.Endpoints.SignIn;

internal class SignInEndpoint : IAuthEndpoint
{
    public AuthEndpointDefinition Definition => new()
    {
        Path = "/sign-in/email",
        Method = HttpMethodType.POST,
        Validator = _ => new SignInValidator(),
        Handler = async ctx =>
        {
            var user =
                await ctx.AuthContext.InternalAdapter.FindUserByEmailAsync(ctx.Body["Email"]!.ToString()!, true);
            
            if (user == null) throw AuthApiException.BadRequest("Account not found.");

            var account = user.Accounts.Find(a => a.ProviderId == "credential");

            if (account == null) throw AuthApiException.BadRequest("Account has to be credential");

            var isPasswordCorrect =
                await ctx.AuthContext.PasswordHasher.VerifyAsync(account.Password!, ctx.Body["Password"]!.ToString()!);

            if (!isPasswordCorrect) throw AuthApiException.BadRequest("Incorrect password");
            
            var session = await ctx.AuthContext.AuthService.CreateSessionAsync(user.Id, new SessionMetadata
            {
                UserAgent = ctx.Request.Headers.UserAgent.ToString(),
                IpAddress = ctx.Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString(),
            }, ctx.HttpContext.RequestServices);

            var cookieOptions = ctx.AuthContext.Options.Session.Cookie;

            cookieOptions.Expires = DateTime.UtcNow.Add(ctx.AuthContext.Options.Session.ExpiresIn);
            
            ctx.SetCookie("better-auth.session_token", session.Token, cookieOptions);

            if (!ctx.AuthContext.Options.EmailVerification.SendOnSignIn || user.EmailVerified)
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
            var emailService = ctx.Resolve<EmailVerificationService>();
            _ = emailService.SendVerificationAsync(user, ctx.Body["callbackURL"]?.ToString() ?? "", ctx.HttpContext);

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