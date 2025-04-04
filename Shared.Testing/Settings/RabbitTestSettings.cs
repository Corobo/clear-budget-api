using Microsoft.AspNetCore.Hosting;
using Shared.Testing.Containers;

namespace Shared.Testing.Settings
{
    public static class RabbitTestSettings
    {
        public static Action<IWebHostBuilder> UseRabbitSettings(RabbitMqContainerFixture rabbit) => builder =>
        {
            builder.UseSetting("RabbitMQ:Host", rabbit.Hostname);
            builder.UseSetting("RabbitMQ:Port", rabbit.Port.ToString());
            builder.UseSetting("RabbitMQ:User", rabbit.Username);
            builder.UseSetting("RabbitMQ:Password", rabbit.Password);
        };
    }

}
