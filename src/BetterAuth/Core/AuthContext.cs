using BetterAuth.Abstractions;
using BetterAuth.Configuration;
using Microsoft.Extensions.Logging;

namespace BetterAuth.Core;

public class AuthContext
{
    public required BetterAuthOptions Options { get; init; }
    public required IInternalAdapter InternalAdapter { get; init; }
    
    public required IAuthDatabaseAdapter DatabaseAdapter { get; init; }
    public required IPasswordHasher PasswordHasher { get; init; }
    public required ILogger Logger { get; init; }
    public required string Secret { get; init; }
}