using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Shared.Logging.Extensions
{
    public static class SerilogExtensions
    {
        public static IHostBuilder UseSharedSerilog(this IHostBuilder builder, string serviceName)
        {
            return builder.UseSerilog((context, config) =>
            {
                config.ReadFrom.Configuration(context.Configuration)
                      .Enrich.FromLogContext()
                      .Enrich.WithProperty("Service", serviceName)
                      .WriteTo.Console()
                      .WriteTo.File(
                          path: $"Logs/{serviceName}-.log",
                          rollingInterval: RollingInterval.Day,
                          retainedFileCountLimit: 10
                      );
            });
        }
    }
}
