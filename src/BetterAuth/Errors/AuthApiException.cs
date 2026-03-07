namespace BetterAuth.Errors;

public class AuthApiException : Exception
{
    public int StatusCode { get; }

    public AuthApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
    
    public static AuthApiException BadRequest(string message) => new(400, message);
    public static AuthApiException Unauthorized(string message = "Unauthorized") => new(401, message);
    public static AuthApiException Forbidden(string message = "Forbidden") => new(403, message);
    public static AuthApiException NotFound(string message = "Not found") => new(404, message);
}