using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Messaging.EventBus.Impl
{
    public class EventBusProducer<T> : IEventBusProducer<T>
    {
        private readonly IConnection _connection;
        private IChannel? _channel;

        public EventBusProducer(IConnection connection)
        {
            _connection = connection;
        }

        public async Task PublishAsync(T @event, string exchange, string routingKey = "")
        {
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);
            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent
            };

            _channel = await GetOrCreateChannelAsync();

            await _channel.BasicPublishAsync(
                exchange,
                routingKey,
                false,
                props,
                body);

        }

        private async Task<IChannel> GetOrCreateChannelAsync()
        {
            if (_channel is not null)
                return _channel;

            _channel = await _connection.CreateChannelAsync().ConfigureAwait(false);
            return _channel;
        }
    }
}
