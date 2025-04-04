using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Shared.Testing.Factories
{
    public class CustomWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
        where TEntryPoint : class
    {
        private readonly Action<IServiceCollection>? _overrideServices;
        private readonly Action<IWebHostBuilder>? _customWebHostBuilder;

        public CustomWebApplicationFactory(Action<IServiceCollection>? overrideServices = null,
            Action<IWebHostBuilder>? customWebHostBuilder = null)
        {
            _overrideServices = overrideServices;
            _customWebHostBuilder = customWebHostBuilder;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            _customWebHostBuilder?.Invoke(builder);

            builder.ConfigureServices(services =>
            {
                _overrideServices?.Invoke(services);
            });

        }
    }
}
