namespace BetterAuth.Events;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, IServiceProvider services)
        where TEvent : IAuthEvent;

    void Subscribe<TEvent>(Func<TEvent, IServiceProvider, Task> handler)
        where TEvent : IAuthEvent;
}
