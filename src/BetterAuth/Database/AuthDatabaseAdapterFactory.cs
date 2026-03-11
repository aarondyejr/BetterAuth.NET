using BetterAuth.Abstractions;

namespace BetterAuth.Database;

public class AuthDatabaseAdapterFactory
{
    public static IAuthDatabaseAdapter Create(
        AdapterConfig config,
        IAuthDatabaseAdapter rawAdapter
        )
    {
        return new TransformingAdapter(config, rawAdapter);
    }
}