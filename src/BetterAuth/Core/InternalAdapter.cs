using System.Security.Cryptography;
using BetterAuth.Abstractions;
using BetterAuth.Database.Args;
using BetterAuth.Configuration;
using BetterAuth.Models;
using BetterAuth.Models.Inputs;

namespace BetterAuth.Core;

public class InternalAdapter(IAuthDatabaseAdapter adapter, BetterAuthOptions options) : IInternalAdapter
{
    public async Task<UserRecord> CreateUserAsync(CreateUserInput input)
    {
        var args = new CreateArgs
        {
            Model = "user",
            Data = new Dictionary<string, object?>
            {
                ["id"] = GenerateId(),
                ["email"] = input.Email,
                ["name"] = input.Name,
                ["image"] = input.Image,
                ["emailVerified"] = false,
                ["createdAt"] = DateTime.UtcNow,
                ["updatedAt"] = DateTime.UtcNow
            }
        };
        
        
        
        return MapToUserRecord(await adapter.CreateAsync(args));
    }

    public async Task<UserWithAccounts?> FindUserByEmailAsync(string email, bool includeAccounts = false)
    {
        var args = new FindOneArgs
        {
            Model = "user",
            Where =
            [
                new WhereClause
                {
                    Field = "email",
                    Operator = WhereOperator.Equals,
                    Value = email
                }

            ]
        };
        
        var rawUser = await adapter.FindOneAsync(args);

        if (rawUser == null) return null;
        
        var user =  MapToUserRecord(rawUser);

        if (includeAccounts)
        {
            var accountArgs = new FindManyArgs()
            {
                Model = "account",
                Where =
                [
                    new WhereClause
                    {
                        Field = "userId",
                        Operator = WhereOperator.Equals,
                        Value = user.Id
                    }
                ]
            };
            
            var rawAccounts =  await adapter.FindManyAsync(accountArgs);
            var accounts = rawAccounts.Select(MapToAccountRecord).ToList();

            return ToUserWithAccounts(user, accounts);
        }

        return ToUserWithAccounts(user, []);
    }

    public async Task<UserRecord?> FindUserByIdAsync(string id)
    {
        var args = new FindOneArgs
        {
            Model = "user",
            Where = [new WhereClause { Field = "id", Operator = WhereOperator.Equals, Value = id }]
        };

        var rawUser = await adapter.FindOneAsync(args);
        
        return rawUser == null ? null : MapToUserRecord(rawUser);
    }

    public async Task<UserRecord> UpdateUserAsync(string id, Dictionary<string, object?> data)
    {
        data.Add("updatedAt", DateTime.UtcNow);
        
        var args = new UpdateArgs
        {
            Model = "user",
            Data = data,
            Where = [new WhereClause { Field = "id", Operator = WhereOperator.Equals, Value = id }]
        };
        
        return MapToUserRecord(await adapter.UpdateAsync(args));
    }

    public async Task<SessionRecord> CreateSessionAsync(string userId, SessionMetadata? metadata)
    {
        var args = new CreateArgs
        {
            Model = "session",
            Data = new Dictionary<string, object?>
            {
                ["id"] = GenerateId(),
                ["userId"] = userId,
                ["token"] = GenerateSessionToken(),
                ["expiresAt"] = DateTime.UtcNow + options.Session.ExpiresIn,
                ["ipAddress"] = metadata?.IpAddress,
                ["userAgent"] =  metadata?.UserAgent,
                ["createdAt"] = DateTime.UtcNow,
                ["updatedAt"] = DateTime.UtcNow
            }
        };
        
        return MapToSessionRecord(await adapter.CreateAsync(args));
    }

    public async Task<SessionRecord?> FindSessionByTokenAsync(string token)
    {
        var args = new FindOneArgs
        {
            Model = "session",
            Where = [new WhereClause { Field = "token", Operator = WhereOperator.Equals, Value = token }]
        };
        
        var rawSession = await adapter.FindOneAsync(args);
        return rawSession == null ? null : MapToSessionRecord(rawSession);
    }

    public async Task<SessionRecord?> RefreshSessionAsync(SessionRecord session, SessionMetadata? metadata)
    {
        var deleted = await DeleteSessionAsync(session.Token);

        if (!deleted) return null;

        return await CreateSessionAsync(session.UserId, metadata);
    }

    public async Task<bool> DeleteSessionAsync(string token)
    {
        var args = new DeleteArgs
        {
            Model = "session",
            Where = [new WhereClause { Field = "token", Operator = WhereOperator.Equals, Value = token }]
        };
        
        return await adapter.DeleteAsync(args);
    }

    public async Task DeleteUserSessionsAsync(string userId)
    {
        var args = new DeleteManyArgs
        {
            Model = "session",
            Where = [new WhereClause { Field = "userId", Operator = WhereOperator.Equals, Value = userId }]
        };
        
        await adapter.DeleteManyAsync(args);
    }

    public async Task<AccountRecord> CreateAccountAsync(CreateAccountInput input)
    {
        var args = new CreateArgs
        {
            Model = "account",
            Data = new Dictionary<string, object?>
            {
                ["id"] = GenerateId(),
                ["userId"] = input.UserId,
                ["accountId"] = input.AccountId,
                ["providerId"] = input.ProviderId,
                ["password"] = input.Password,
                ["createdAt"] = DateTime.UtcNow,
                ["updatedAt"] = DateTime.UtcNow
            }
        };
        
        return MapToAccountRecord(await adapter.CreateAsync(args));
    }

    public async Task<AccountRecord?> FindAccountByProviderAsync(string providerId, string accountId)
    {
        var args = new FindOneArgs
        {
            Model = "account",
            Where =
            [
                new WhereClause { Field = "providerId", Operator = WhereOperator.Equals, Value = providerId },
                new WhereClause { Field = "accountId", Operator = WhereOperator.Equals, Value = accountId }
            ]
        };
        
        var rawAccount = await adapter.FindOneAsync(args);
        
        return rawAccount == null ? null : MapToAccountRecord(rawAccount);
    }

    public async Task<VerificationRecord> CreateVerificationValueAsync(CreateVerificationInput input)
    {
        var args = new CreateArgs
        {
            Model = "verification",
            Data = new Dictionary<string, object?>
            {
                ["id"] = GenerateId(),
                ["identifier"] = input.Identifier,
                ["value"] = GenerateSessionToken(),
                ["expiresAt"] = input.ExpiresAt,
                ["createdAt"] = DateTime.UtcNow,
                ["updatedAt"] = DateTime.UtcNow
            }
        };
        
        return MapToVerificationRecord(await adapter.CreateAsync(args));
    }

    public async Task<VerificationRecord?> FindVerificationValueAsync(string token)
    {
        var args = new FindOneArgs
        {
            Model = "verification",
            Where = [new WhereClause { Field = "value", Operator = WhereOperator.Equals, Value = token }]
        };
        
        var rawVerification = await adapter.FindOneAsync(args);
        
        return rawVerification == null ? null : MapToVerificationRecord(rawVerification);
    }

    public async Task DeleteVerificationByIdentifierAsync(string token)
    {
        var args = new DeleteArgs
        {
            Model = "verification",
            Where = [new WhereClause { Field = "value", Operator = WhereOperator.Equals, Value = token }]
        };
        
        await  adapter.DeleteAsync(args);
    }

    public async Task UpdateVerificationByIdentifierAsync(string identifier, Dictionary<string, object?> data)
    {
        data.Add("updatedAt", DateTime.UtcNow);
        
        var args = new UpdateArgs
        {
            Model = "verification",
            Where = [new WhereClause { Field = "identifier", Operator = WhereOperator.Equals, Value = identifier }],
            Data = data
        };

        await adapter.UpdateAsync(args);
    }

    public async Task UpdatePasswordAsync(string userId, string hashedPassword)
    {
        var args = new UpdateArgs
        {
            Model = "account",
            Where =
            [
                new WhereClause { Field = "userId", Operator = WhereOperator.Equals, Value = userId },
                new WhereClause { Field = "providerId", Operator = WhereOperator.Equals, Value = "credential" }
            ],
            Data = new Dictionary<string, object?>
            {
                ["password"] = hashedPassword,
                ["updatedAt"] = DateTime.UtcNow
            }
        };
        
        await adapter.UpdateAsync(args);
    }
    
    private UserRecord MapToUserRecord(Dictionary<string, object?> data)
    {
        return new UserRecord
        {
            Id = (string)data["id"]!,
            Email = (string)data["email"]!,
            Name = (string)data["name"]!,
            EmailVerified = (bool)data["emailVerified"]!,
            Image = data["image"] as string,
            CreatedAt = (DateTime)data["createdAt"]!,
            UpdatedAt = (DateTime)data["updatedAt"]!,
        };
    }

    private SessionRecord MapToSessionRecord(Dictionary<string, object?> data)
    {
        return new SessionRecord
        {
            Id = (string)data["id"]!,
            UserId = (string)data["userId"]!,
            Token = (string)data["token"]!,
            IpAddress = (string?)data["ipAddress"],
            UserAgent = (string?)data["userAgent"],
            ExpiresAt = (DateTime)data["expiresAt"]!,
            CreatedAt = (DateTime)data["createdAt"]!,
            UpdatedAt = (DateTime)data["updatedAt"]!
        };
    }

    private AccountRecord MapToAccountRecord(Dictionary<string, object?> data)
    {
        return new AccountRecord
        {
            Id = (string)data["id"]!,
            UserId = (string)data["userId"]!,
            AccountId = (string)data["accountId"]!,
            ProviderId = (string)data["providerId"]!,
            Password = (string?)data["password"],
            CreatedAt = (DateTime)data["createdAt"]!,
            UpdatedAt = (DateTime)data["updatedAt"]!
        };

    }

    private VerificationRecord MapToVerificationRecord(Dictionary<string, object?> data)
    {
        return new VerificationRecord
        {
            Id = (string)data["id"]!,
            Identifier = (string)data["identifier"]!,
            Value = (string)data["value"]!,
            ExpiresAt = (DateTime)data["expiresAt"]!,
            CreatedAt = (DateTime)data["createdAt"]!,
            UpdatedAt = (DateTime)data["updatedAt"]!
        };
    }
    
    private UserWithAccounts ToUserWithAccounts(UserRecord user, List<AccountRecord> accounts)
    {
        return new UserWithAccounts
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            EmailVerified = user.EmailVerified,
            Image = user.Image,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Accounts = accounts,
        };
    }
    
    private string GenerateId()
    {
        return Guid.NewGuid().ToString();
    }

    private string GenerateSessionToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}