using BetterAuth.Models;

namespace BetterAuth.Events.Auth;

public record SessionExpiredEvent(SessionRecord Session) : IAuthEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
