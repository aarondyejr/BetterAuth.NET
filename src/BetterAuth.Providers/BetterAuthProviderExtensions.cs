using BetterAuth.Abstractions;
using BetterAuth.Providers.Configuration;
using BetterAuth.Providers.Email;
using BetterAuth.Providers.Storage;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace BetterAuth.Providers;

public static class BetterAuthProviderExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddBetterAuthStorage(S3StorageOptions options)
        {
            services.AddMinio(configureClient => configureClient
                .WithEndpoint(options.Endpoint)
                .WithCredentials(options.AccessKey, options.SecretKey)
                .WithSSL(options.UseSsl)
                .Build());

            services.AddSingleton<IStorageProvider>(sp => new S3StorageProvider(sp.GetRequiredService<IMinioClient>(), options));

            return services;
        }

        public IServiceCollection AddBetterAuthSmtp(SmtpOptions options)
        {
            services.AddSingleton<IEmailProvider>(new SmtpProvider(options));
            
            return services;
        }
    }
}