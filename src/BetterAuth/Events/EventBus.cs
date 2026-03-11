using System.Collections.Concurrent;

namespace BetterAuth.Events;

public class EventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<Func<IAuthEvent, IServiceProvider, Task>>> _handlers = new();

    public async Task PublishAsync<TEvent>(TEvent @event, IServiceProvider services)
        where TEvent : IAuthEvent
    {
        if (!_handlers.TryGetValue(typeof(TEvent), out var handlers))
            return;

        // use foreach to avoid conflicting scoped services if two handlers for the same event are registered, I doubt it will happen so I am using WhenAll for speed and concurency.
        var tasks = handlers.Select(h => h(@event, services));
        await Task.WhenAll(tasks);
    }

    public void Subscribe<TEvent>(Func<TEvent, IServiceProvider, Task> handler)
        where TEvent : IAuthEvent
    {
        _handlers.AddOrUpdate(
            typeof(TEvent),
            _ => [(e, sp) => handler((TEvent)e, sp)],
            (_, existing) =>
            {
                lock (existing)
                {
                    existing.Add((e, sp) => handler((TEvent)e, sp));
                }
                return existing;
            }
        );
    }
}
