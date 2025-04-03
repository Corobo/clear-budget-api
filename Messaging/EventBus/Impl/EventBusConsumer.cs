using RabbitMQ.Client.Events;
using Serilog;

namespace Messaging.EventBus.Impl
{
    public abstract class EventBusConsumer<T> : IEventBusConsumer<T>
    {
        private readonly ILogger _logger;

        protected EventBusConsumer(ILogger logger)
        {
            _logger = logger;
        }

        public async Task ConsumeAsync(BasicDeliverEventArgs @event, T context)
        {
            var eventType = context?.GetType().Name ?? typeof(T).Name;

            try
            {
                _logger.Information("Handling event of type {eventType}", eventType);

                if (context == null)
                {
                    _logger.Warning("Received null context for event type {eventType}", eventType);
                    return;
                }

                await HandleAsync(context);

                _logger.Information("Handled event of type {eventType} successfully", eventType);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while handling event of type {eventType}", eventType);
                throw;
            }
        }

        protected abstract Task HandleAsync(T context);
    }
}
