using BetterAuth.Models;

namespace BetterAuth.Events.Auth;

public class SessionCreatedEvent(SessionRecord Session) : IAuthEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}