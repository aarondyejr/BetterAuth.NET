using BetterAuth.Models;

namespace BetterAuth.Events.Auth;

public record UserSignedOutEvent(UserRecord User, string SessionToken) : IAuthEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
