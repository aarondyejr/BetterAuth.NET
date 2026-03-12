namespace BetterAuth.Abstractions;

public interface IEmailTemplate
{
    string Subject { get; }
    string Render(Dictionary<string, string> variables);
}