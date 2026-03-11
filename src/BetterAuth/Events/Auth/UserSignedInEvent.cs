using BetterAuth.Models;

namespace BetterAuth.Events.Auth;

public record UserSignedInEvent(UserRecord User, SessionRecord Session) : IAuthEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
