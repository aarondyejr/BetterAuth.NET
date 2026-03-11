using BetterAuth.Abstractions;
using BetterAuth.Events;
using BetterAuth.Events.Auth;
using BetterAuth.Models;
using BetterAuth.Models.Inputs;

namespace BetterAuth.Services;

public class AuthService(IInternalAdapter adapter, IEventBus eventBus)
{
    public async Task<UserRecord> CreateUserAsync(CreateUserInput input, IServiceProvider services)
    {
        var user = await adapter.CreateUserAsync(input);

        await eventBus.PublishAsync(new UserCreatedEvent(user), services);

        return user;
    }

    public async Task<SessionRecord> CreateSessionAsync(string userId, SessionMetadata? metadata,
        IServiceProvider services)
    {
        var session = await adapter.CreateSessionAsync(userId, metadata);

        await eventBus.PublishAsync(new SessionCreatedEvent(session), services);
        
        return session;
    }

    public async Task DeleteSessionAsync(string token, UserRecord user, IServiceProvider services)
    {
        await adapter.DeleteSessionAsync(token);
        await eventBus.PublishAsync(new UserSignedOutEvent(user, token), services);
    }
}