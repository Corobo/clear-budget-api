using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

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