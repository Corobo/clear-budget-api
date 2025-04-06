using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
using Serilog;
using Microsoft.Extensions.Hosting;

namespace Shared.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IHostEnvironment _env;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger logger, IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _env = environment;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var traceId = context.TraceIdentifier;

                _logger.Error(ex, "Unhandled exception in {Path} - TraceId: {TraceId}", context.Request.Path, traceId);

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                object response;

                if (_env.IsDevelopment())
                {
                    response = new
                    {
                        message = ex.Message,
                        traceId,
                        exception = ex.GetType().Name,
                        stackTrace = ex.StackTrace
                    };
                }
                else
                {
                    response = new
                    {
                        message = "An unexpected error occurred. Please try again later.",
                        traceId
                    };
                }


                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
            }
        }
    }
}
