using Shared.Messaging.Background;
using Shared.Messaging.Configuration;
using Shared.Messaging.Events;
using Shared.Messaging.Factories;
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
