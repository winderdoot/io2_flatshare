namespace flatshare_server.Infrastructure.Model.Responses;

public record FieldError(string Field, string Message);

public record ValidationErrorResponse(DateTime Timestamp, int Status, string Error, IEnumerable<FieldError> FieldErrors);
