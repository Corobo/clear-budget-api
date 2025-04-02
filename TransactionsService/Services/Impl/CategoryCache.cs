using Polly.Retry;
using Polly;
using System.Collections.Concurrent;
using TransactionsService.Clients;
using System.Threading;

namespace TransactionsService.Services.Impl
{
    public class CategoryCache : ICategoryCache
    {
        private readonly ICategoriesClient _categoriesClient;
        private readonly ConcurrentDictionary<Guid, bool> _categories = new();

        public CategoryCache(ICategoriesClient categoriesClient)
        {
            _categoriesClient = categoriesClient;
        }

        public async Task InitializeAsync()
        {

            // === Polly ===
            var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>(),
                    MaxRetryAttempts = 5,
                    Delay = TimeSpan.FromSeconds(2),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                })
                .Build();

            var ids = await pipeline.ExecuteAsync(async ct => 
            await _categoriesClient.GetAllCategoryIdsAsync(ct), cancellationToken);
            _categories.Clear();

            foreach (var id in ids)
            {
                _categories.TryAdd(id, true);
            }
        }

        public async Task RefreshAsync()
        {
            await InitializeAsync();
        }

        public bool IsValidCategory(Guid categoryId)
        {
            return _categories.ContainsKey(categoryId);
        }
    }
}
