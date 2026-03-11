using BetterAuth.Models;

namespace BetterAuth.Events.Auth;

public record UserCreatedEvent(UserRecord User) : IAuthEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
