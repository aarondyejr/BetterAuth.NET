using BetterAuth.Abstractions;
using BCrypt.Net;

namespace BetterAuth.Crypto;

public class BCryptPasswordHasher : IPasswordHasher
{
    public Task<string> HashAsync(string password)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        
        return Task.FromResult(passwordHash);
    }

    public Task<bool> VerifyAsync(string hash, string password)
    {
        var isValid = BCrypt.Net.BCrypt.Verify(password, hash);
        
        return Task.FromResult(isValid);
    }
}