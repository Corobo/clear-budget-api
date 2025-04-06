using Shared.Messaging.Configuration;
using Shared.Messaging.Connection;
using Shared.Messaging.EventBus;
using Shared.Messaging.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Shared.Messaging.Background
{
    public abstract class SubscriptionService<T> : BackgroundService where T : class
    {
        protected string QueueName;

        private readonly ILogger _logger;
        private IEventBusConsumer<T> _consumer;
        protected readonly IServiceScopeFactory _scopeFactory;
        private IConnection? _connection;
        private IChannel? _channel;
        public readonly IOptions<RabbitMQOptions> _options;
        private readonly RabbitMqConnectionFactory _rabbitMqConnectionFactory;

        public SubscriptionService(ILogger logger, IOptions<RabbitMQOptions> options, IServiceScopeFactory scopeFactory,
            RabbitMqConnectionFactory rabbitMqConnectionFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _options = options;
            _rabbitMqConnectionFactory = rabbitMqConnectionFactory;
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();

            base.Dispose();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _connection = await _rabbitMqConnectionFactory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false).ConfigureAwait(false);

                AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += OnMessageReceived;


                await _channel.BasicConsumeAsync(QueueName, false, consumer).ConfigureAwait(false);
                _logger.Information("Subscribed to RabbitMQ messages on queue {queueName}", QueueName);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.Error("Not able to subscribe to RabbitMQ messages on queue {queueName}, exception {exception}", QueueName, ex.Message);
                await Task.CompletedTask;
            }
        }

        private async Task OnMessageReceived(object sender, BasicDeliverEventArgs @event)
        {
            _logger.Information("Received RabbitMQ message [RoutingKey: {routingKey} DeliveryTag: {deliveryTag}]", @event.RoutingKey, @event.DeliveryTag);
            try
            {
                await ProcessMessage(@event).ConfigureAwait(false);
                await _channel!.BasicAckAsync(@event.DeliveryTag, multiple: false).ConfigureAwait(false);
                _logger.Information("Processed and acknowledged RabbitMQ message [RoutingKey: {routingKey} DeliveryTag: {deliveryTag}]", @event.RoutingKey, @event.DeliveryTag);
            }
            catch (Exception exception)
            {
                bool isRetriable = false;

                bool requeue = isRetriable;
                await _channel!.BasicRejectAsync(@event.DeliveryTag, requeue).ConfigureAwait(false);
                _logger.Error(exception, "Could not process RabbitMQ message [RoutingKey: {routingKey} DeliveryTag: {deliveryTag}]", @event.RoutingKey, @event.DeliveryTag);
                _logger.Warning("Rejected RabbitMQ message [RoutingKey: {routingKey} DeliveryTag: {deliveryTag} Requeue: {requeue}]", @event.RoutingKey, @event.DeliveryTag, requeue);
                throw;
            }
        }

        protected virtual async Task ProcessMessage(BasicDeliverEventArgs @event)
        {
            using var scope = _scopeFactory.CreateScope();
            var consumer = scope.ServiceProvider.GetRequiredService<IEventBusConsumer<T>>();

            var body = @event.Body.ToArray();
            var eventMessage = Encoding.UTF8.GetString(body);
            _logger.Information("RabbitMQ [Message]: {message}", eventMessage);

            try
            {
                T context = JObject.Parse(eventMessage).ToObject<T>();
                if (context != null)
                {
                    await consumer.ConsumeAsync(@event, context).ConfigureAwait(false);
                    await Task.Delay(1500);
                }
                else
                {
                    _logger.Error("Deserialized context is null for message [RoutingKey: {routingKey} DeliveryTag: {deliveryTag}]", @event.RoutingKey, @event.DeliveryTag);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error processing message [RoutingKey: {routingKey} DeliveryTag: {deliveryTag}]", @event.RoutingKey, @event.DeliveryTag);
            }
        }
    }
}
