using BetterAuth.Abstractions;
using BetterAuth.Configuration;
using BetterAuth.Providers.Configuration;
using MailKit.Net.Smtp;
using MimeKit;

namespace BetterAuth.Providers.Email;

public class SmtpProvider(SmtpOptions options) : IEmailProvider
{
    public async Task SendAsync(EmailMessage message)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(message.From));
        email.To.Add(MailboxAddress.Parse(message.To));
        email.Subject = message.Subject;
        
        var bodyBuilder = new BodyBuilder();
        if (message.Html is not null) bodyBuilder.HtmlBody = message.Html;
        if (message.Text is not null) bodyBuilder.TextBody = message.Text;
        email.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(options.Host, options.Port, options.UseSsl);
        await client.AuthenticateAsync(options.Username, options.Password);
        await client.SendAsync(email);
        await client.DisconnectAsync(true);
    }
}