using Messaging.Configuration;
using Messaging.Factories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.Connection
{
    public class RabbitMqConnectionInitializer : BackgroundService
    {
        private readonly RabbitMqConnectionFactory _factory;
        private readonly RabbitMqConnectionAccessor _accessor;

        public RabbitMqConnectionInitializer(RabbitMqConnectionFactory factory, RabbitMqConnectionAccessor accessor)
        {
            _factory = factory;
            _accessor = accessor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _accessor.EnsureConnectionInitializedAsync();
        }
    }

}
