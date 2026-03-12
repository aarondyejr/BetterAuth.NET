using BetterAuth.Models;

namespace BetterAuth.Events.Auth;

public record SessionCreatedEvent(SessionRecord Session) : IAuthEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}