using BetterAuth.Models;
using BetterAuth.Models.Inputs;

namespace BetterAuth.Abstractions;

public interface IInternalAdapter
{
    Task<UserRecord> CreateUserAsync(CreateUserInput input);
    Task<UserWithAccounts?> FindUserByEmailAsync(string email, bool includeAccounts = false);
    Task<UserRecord?> FindUserByIdAsync(string id);
    Task<UserRecord> UpdateUserAsync(string id, Dictionary<string, object?> data);
    
    Task<SessionRecord> CreateSessionAsync(string userId, SessionMetadata? metadata);
    Task<SessionRecord?> FindSessionByTokenAsync(string token);
    Task DeleteSessionAsync(string token);
    Task DeleteUserSessionsAsync(string userId);

    Task<AccountRecord> CreateAccountAsync(CreateAccountInput input);
    Task<AccountRecord?> FindAccountByProviderAsync(string providerId, string accountId);
    
    Task<VerificationRecord> CreateVerificationValueAsync(CreateVerificationInput input);
    Task<VerificationRecord?> FindVerificationValueAsync(string identifier);
    Task DeleteVerificationByIdentifierAsync(string identifier);
    Task UpdateVerificationByIdentifierAsync(string identifier, Dictionary<string, object?> data);


    Task UpdatePasswordAsync(string userId, string hashedPassword);
}