namespace BetterAuth.Endpoints.SignUp;

public class SignUpRequest
{
    public string Email { get;init; } = "";
    public string Name { get; init; } = "";
    public string Password { get;init; } = "";
    public string? Image { get;init; }
    public string? CallbackUrl { get;init; }
}