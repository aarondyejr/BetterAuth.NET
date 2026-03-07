using BetterAuth.Core;
using BetterAuth.Plugins;

namespace BetterAuth.Endpoints;

internal class SignInEndpoint : IAuthEndpoint
{
    public AuthEndpointDefinition Definition => new()
    {
        Path = "/sign-in/email",
        Method = HttpMethodType.POST,
        Handler = async ctx =>
        {
            Console.WriteLine();

            return ctx.User ?? new()
            {
                Email = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, EmailVerified = false, Id = "123",
                Name = "John Doe", Image = ""
            };
        }
    };
}