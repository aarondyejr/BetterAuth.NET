using BetterAuth.Configuration;

namespace BetterAuth.Abstractions;

public interface IEmailProvider
{
    public Task SendAsync(EmailMessage message);
}