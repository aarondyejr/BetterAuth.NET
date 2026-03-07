using BetterAuth.Core;
using BetterAuth.Plugins;
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
        Handler = async ctx => ctx.Json(new Dictionary<string, string> { ["success"] = "true" })
    };
}