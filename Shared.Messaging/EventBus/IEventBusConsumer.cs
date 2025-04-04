using RabbitMQ.Client.Events;

namespace Shared.Messaging.EventBus
{
    public interface IEventBusConsumer<T>
    {
        Task ConsumeAsync(BasicDeliverEventArgs @event, T context);
    }

}
