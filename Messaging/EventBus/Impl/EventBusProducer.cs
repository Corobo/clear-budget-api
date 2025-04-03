using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Messaging.Connection;

namespace Messaging.EventBus.Impl
{
    public class EventBusProducer<T> : IEventBusProducer<T>
    {
        private readonly RabbitMqConnectionAccessor _accessor;
        private IChannel? _channel;

        public EventBusProducer(RabbitMqConnectionAccessor accessor)
        {
            _accessor = accessor;
        }

        public async Task PublishAsync(T @event, string exchange, string routingKey = "")
        {
            if (_accessor.Connection is null)
                throw new InvalidOperationException("RabbitMQ connection not initialized.");

            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);
            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent
            };

            _channel ??= await _accessor.Connection.CreateChannelAsync().ConfigureAwait(false);

            await _channel.BasicPublishAsync(
                exchange,
                routingKey,
                mandatory: false,
                props,
                body
            );
        }
    }
}
