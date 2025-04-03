namespace TransactionsService.Clients.Impl
{
    public class CategoriesClient : ICategoriesClient
    {
        private readonly HttpClient _httpClient;

        public CategoriesClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<Guid>> GetAllCategoryIdsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<Guid>>("/api/categories/ids", cancellationToken);
                return result ?? new List<Guid>();
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return Enumerable.Empty<Guid>();
            }
        }
    }
}