namespace Shared.Messaging.EventBus
{
    public interface IEventBusProducer<T>
    {
        Task PublishAsync(T @event, string exchange, string routingKey = "");
    }
}
