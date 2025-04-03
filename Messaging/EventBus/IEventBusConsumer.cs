using RabbitMQ.Client.Events;

namespace Messaging.EventBus
{
    public interface IEventBusConsumer<T>
    {
        Task ConsumeAsync(BasicDeliverEventArgs @event, T context);
    }

}
