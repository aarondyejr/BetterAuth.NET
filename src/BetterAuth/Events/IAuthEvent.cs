namespace BetterAuth.Events;

public interface IAuthEvent
{
    DateTime OccurredAt { get; }
}
