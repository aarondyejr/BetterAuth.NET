namespace BetterAuth.Abstractions;

public interface IStorageProvider
{
    Task<string> UploadAsync(string key, Stream content, string contentType);
    Task DeleteAsync(string key);
    Task<string> GetUrlAsync(string key);
}