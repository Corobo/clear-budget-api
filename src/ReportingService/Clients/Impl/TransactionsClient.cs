using ReportingService.Models.DTO;

namespace ReportingService.Clients.Impl
{
    public class TransactionsClient : ITransactionsClient
    {
        private readonly HttpClient _httpClient;

        public TransactionsClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<TransactionDTO>> GetUserTransactionsAsync(Guid userId,
            CancellationToken cancellationToken = default)
        {
            var requestUri = $"/api/transactions?userId={userId.ToString()}"; 
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            try
            {
                using (var response = await _httpClient.SendAsync(request, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadFromJsonAsync<IEnumerable<TransactionDTO>>(cancellationToken)
                               ?? Enumerable.Empty<TransactionDTO>();
                }
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return Enumerable.Empty<TransactionDTO>();
            }
        }
    }


}