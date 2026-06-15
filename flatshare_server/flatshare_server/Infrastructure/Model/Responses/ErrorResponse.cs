using flatshare_server.Infrastructure.Model.Exceptions;

namespace flatshare_server.Infrastructure.Model.Responses;

public record FieldError(string Field, string Message);

/* Standardized server API response */
public record class ErrorResponse
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public required int Status { get; init; }
    public required string Error { get; init; }

    /* Optional, used only for 400 Bad Request validation errors */
    public List<FieldError>? FieldErrors { get; init; } = null;
    public static ServerResponseException Generate(string errorName, int statusCode = StatusCodes.Status400BadRequest, List<FieldError>? fields = null)
    {
        return new ServerResponseException(
            new ErrorResponse
            {
                Status = statusCode,
                Error = errorName,
                FieldErrors = fields
            }
        );
    }
}
