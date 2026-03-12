using BetterAuth.Abstractions;
using BetterAuth.Providers.Configuration;
using Minio;
using Minio.DataModel.Args;

namespace BetterAuth.Providers.Storage;

public class S3StorageProvider(IMinioClient client, S3StorageOptions options) : IStorageProvider
{
    public async Task<string> UploadAsync(string key, Stream content, string contentType)
    {
        await client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(options.Bucket)
            .WithObject(key)
            .WithStreamData(content)
            .WithObjectSize(content.Length)
            .WithContentType(contentType));

        return await GetUrlAsync(key);
    }

    public async Task DeleteAsync(string key)
    {
        await client.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(options.Bucket).WithObject(key));
    }

    public async Task<string> GetUrlAsync(string key)
    {
        if (options.PrivateBucket)
        {
            return await client.PresignedGetObjectAsync(new PresignedGetObjectArgs().WithBucket(options.Bucket)
                .WithObject(key).WithExpiry(options.PresignedUrlExpirySeconds));
        }

        var protocol = options.UseSsl ? "https" : "http";
        return $"{protocol}://{options.Endpoint}/{options.Bucket}/{key}";
    }
}