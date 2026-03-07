namespace BetterAuth.Abstractions;

public interface IPasswordHasher
{
    Task<string> HashAsync(string password);
    Task<bool> VerifyAsync(string hash, string password);
}