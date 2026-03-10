using BetterAuth.Core;
using BetterAuth.Models;
using Microsoft.AspNetCore.Http;

namespace BetterAuth.Plugins;

public class AuthEndpointContext
{
    public required HttpRequest Request { get; init; }

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
        Request.HttpContext.Response.Cookies.Append(name, value, options);
    }

    public string? GetCookie(string name)
    {
        return Request.HttpContext.Request.Cookies.TryGetValue(name, out var value) ? value : null;
    }
}