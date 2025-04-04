using Shared.Messaging.EventBus.Impl;
using Shared.Messaging.Events;
using TransactionsService.Services;
using ILogger = Serilog.ILogger;

namespace TransactionsService.Messaging.Consumers
{
    public class CategoryEventConsumer : EventBusConsumer<CategoryEvent>
    {
        private readonly ICategoryCache _cache;

        public CategoryEventConsumer(ICategoryCache cache, ILogger logger)
            : base(logger)
        {
            _cache = cache;
        }

        protected override Task HandleAsync(CategoryEvent context)
        {
            return _cache.RefreshAsync();
        }
    }

}
