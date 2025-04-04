using Shared.Messaging.Factories;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Shared.Messaging.Connection
{
    public class RabbitMqConnectionAccessor
    {
        private readonly RabbitMqConnectionFactory _factory;
        private readonly ILogger<RabbitMqConnectionAccessor> _logger;

        public IConnection? Connection { get; private set; }

        public RabbitMqConnectionAccessor(
            RabbitMqConnectionFactory factory,
            ILogger<RabbitMqConnectionAccessor> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public async Task EnsureConnectionInitializedAsync()
        {
            if (Connection is { IsOpen: true })
            {
                _logger.LogInformation("RabbitMQ connection already initialized.");
                return;
            }

            _logger.LogInformation("Initializing RabbitMQ connection...");
            Connection = await _factory.CreateConnectionAsync();
        }

        public async Task<IConnection> GetOrCreateConnectionAsync()
        {
            if (Connection is { IsOpen: true })
                return Connection;

            _logger.LogWarning("No active RabbitMQ connection. Creating one...");
            Connection = await _factory.CreateConnectionAsync();
            return Connection;
        }
    }

}
