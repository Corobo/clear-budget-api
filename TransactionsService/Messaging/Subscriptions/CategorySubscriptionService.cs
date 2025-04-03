using Messaging.Background;
using Messaging.Configuration;
using Messaging.Events;
using Microsoft.Extensions.Options;
using ILogger = Serilog.ILogger;

namespace TransactionsService.Messaging.Subscriptions
{
    public class CategorySubscriptionService : SubscriptionService<CategoryEvent>
    {
        public CategorySubscriptionService(
            ILogger logger,
            IOptions<RabbitMQOptions> options,
            IServiceScopeFactory scopeFactory)
            : base(logger, options, scopeFactory)
        {
            QueueName = options.Value.QueueNames["Category"];
        }
    }
}
