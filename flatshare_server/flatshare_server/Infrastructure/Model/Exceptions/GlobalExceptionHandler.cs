namespace flatshare_server.Infrastructure.Model.Exceptions;
using flatshare_server.Infrastructure.Model.Responses;
using Microsoft.AspNetCore.Diagnostics;

/* Used globally to intercept all errors that happen, even before controller code is executed */
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        /* If we are here, it means it's a real 500 Internal Server Error. */
        
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        
        var errorResponse = new ErrorResponse
        {
            Status = 500,
            Error = "An unexpected server error occurred."
        };

        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

        return true; 
    }
}