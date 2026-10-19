using SalesSystem.Utility;

namespace SalesSystem.API.Utility
{
    /// <summary>
    /// Turns unhandled exceptions into the standard Response envelope. Only BusinessException
    /// messages reach the client; everything else is logged and answered with a generic 500.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (BusinessException ex)
            {
                _logger.LogInformation("Business rule rejected {Path}: {Message}", context.Request.Path, ex.Message);
                await WriteError(context, ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error processing {Method} {Path}", context.Request.Method, context.Request.Path);
                await WriteError(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        private static async Task WriteError(HttpContext context, int statusCode, string message)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(new Response<object?> { status = false, message = message });
        }
    }
}
