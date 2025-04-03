using Messaging.Background;
using Messaging.Configuration;
using Messaging.Events;
using Messaging.Factories;
using Microsoft.Extensions.Options;
using ILogger = Serilog.ILogger;

namespace TransactionsService.Messaging.Subscriptions
{
    public class CategorySubscriptionService : SubscriptionService<CategoryEvent>
    {
        public CategorySubscriptionService(
            ILogger logger,
            IOptions<RabbitMQOptions> options,
            IServiceScopeFactory scopeFactory,
            RabbitMqConnectionFactory rabbitMqConnectionFactory)
            : base(logger, options, scopeFactory, rabbitMqConnectionFactory)
        {
            QueueName = options.Value.QueueNames["Category"];
        }
    }
}
