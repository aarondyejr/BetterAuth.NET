using BetterAuth.Core;
using BetterAuth.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BetterAuth.Plugins;

public class AuthEndpointContext
{
    public required HttpContext HttpContext { get; init; }
    public HttpRequest Request => HttpContext.Request;

    public required AuthContext AuthContext { get; init; }

    public Dictionary<string, object?> Body { get; init; } = new();

    public Dictionary<string, string> Query { get; init; } = new();

    public required string Path { get; init; }
    
    public string BaseUrl { get; init; }

    public SessionRecord? Session { get; init; }

    public UserRecord? User { get; init; }


    public object Json(object data) => data;

    public void SetCookie(string name, string value, CookieOptions options)
    {
        HttpContext.Response.Cookies.Append(name, value, options);
    }

    public string? GetCookie(string name)
    {
        return HttpContext.Request.Cookies.TryGetValue(name, out var value) ? value : null;
    }

    public void DeleteCookie(string name)
    {
        HttpContext.Response.Cookies.Delete(name);
    }

    public T Resolve<T>()
    {
        return HttpContext.RequestServices.GetRequiredService<T>();
    }

    public bool IsValidCallbackUrl(string callbackUrl)
    {
        if (callbackUrl.StartsWith('/') && !callbackUrl.StartsWith("//"))
            return true;

        if (!Uri.TryCreate(callbackUrl, UriKind.Absolute, out var uri))
            return false;

        var origin = $"{uri.Scheme}://{uri.Host}";

        if (uri.Port is not (443 or 80))
            origin += $":{uri.Port}";

        return AuthContext.Options.TrustedOrigins?.Contains(origin, StringComparer.OrdinalIgnoreCase) ?? false;
    }
}