namespace BetterAuth.Errors;

public class AuthApiException : Exception
{
    public int StatusCode { get; }
    public object Payload { get; }

    public AuthApiException(int statusCode, object payload) : base(payload.ToString())
    {
        StatusCode = statusCode;
        Payload = payload;
    }
    
    public static AuthApiException BadRequest(string message) =>
        new(400, message);

    public static AuthApiException BadRequest(List<ValidationError> errors) =>
        new(400, errors);
    public static AuthApiException Unauthorized(string message = "Unauthorized") => new(401, message);
    public static AuthApiException Forbidden(string message = "Forbidden") => new(403, message);
    public static AuthApiException NotFound(string message = "Not found") => new(404, message);
}