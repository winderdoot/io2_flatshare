namespace flatshare_server.Infrastructure.Model.Exceptions;

public class DomainExceptionMiddleware
{
    private readonly RequestDelegate _next;
    public DomainExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ServerResponseException ex)
        {
            context.Response.StatusCode = ex.Response.Status;
            await context.Response.WriteAsJsonAsync(ex.Response);
        }
    }
}

public static class DomainExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseDomainExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<DomainExceptionMiddleware>();
    }
}