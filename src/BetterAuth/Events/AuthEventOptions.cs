using BetterAuth.Events.Auth;

namespace BetterAuth.Events;

public class AuthEventOptions
{
    public Func<UserCreatedEvent, IServiceProvider, Task>? OnUserCreated { get; init; }
    public Func<UserSignedInEvent, IServiceProvider, Task>? OnUserSignedIn { get; init; }
    public Func<UserSignedOutEvent, IServiceProvider, Task>? OnUserSignedOut { get; init; }
    public Func<SessionExpiredEvent, IServiceProvider, Task>? OnSessionExpired { get; init; }
    public Func<SessionCreatedEvent, IServiceProvider, Task>? OnSessionCreated { get; init; }

    internal void RegisterAll(IEventBus bus)
    {
        if (OnUserCreated is { } h1) bus.Subscribe(h1);
        if (OnUserSignedIn is { } h2) bus.Subscribe(h2);
        if (OnUserSignedOut is { } h3) bus.Subscribe(h3);
        if (OnSessionExpired is { } h4) bus.Subscribe(h4);
        if (OnSessionCreated is { } h5) bus.Subscribe(h5);
    }
}