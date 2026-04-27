using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Utils.Models;

namespace Utils.Middlewares
{
    /// <summary>
    /// Tüm API exceptions'larını merkezi olarak yakalar ve işler
    /// Program.cs'te: app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "API exception yakalandı!");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                ArgumentNullException => StatusCodes.Status400BadRequest,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                InvalidOperationException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorDetails = new ErrorDetails
            {
                StatusCode = context.Response.StatusCode,
                Message = ex.Message,
                StackTrace = ex.StackTrace
            };

            return context.Response.WriteAsync(errorDetails.ToString());
        }
    }
}