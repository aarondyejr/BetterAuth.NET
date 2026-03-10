namespace BetterAuth.Endpoints.SignIn;

public class SignInRequest
{
    public string Email { get; init; } = "";
    public string Password { get; init; } = "";
    public string? CallbackUrl { get; init; }
}