namespace BetterAuth.Providers.Configuration;

public class S3StorageOptions
{
    public required string Endpoint { get; init; }
    public required string AccessKey { get; init; }
    public required string SecretKey { get; init; }
    public required string Bucket { get; init; }
    public bool UseSsl { get; init; } = true;
    public bool PrivateBucket { get; init; } = false;
    public int PresignedUrlExpirySeconds { get; init; } = 3600;
}