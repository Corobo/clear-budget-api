using Microsoft.AspNetCore.Builder;
using Serilog;

namespace Shared.Middleware.Extensions
{
    public static class AppBuilderExtensions
    {
        public static IApplicationBuilder UseSharedMiddlewares(this IApplicationBuilder app)
        {
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseSerilogRequestLogging();
            return app;
        }
    }
}
