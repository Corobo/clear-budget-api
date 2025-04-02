using ReportingService.Models.DTO;
using System.Net.Http.Json;

namespace ReportingService.Clients.Impl
{
    public class TransactionsClient : ITransactionsClient
    {
        private readonly HttpClient _httpClient;

        public TransactionsClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<TransactionDTO>> GetUserTransactionsAsync(string userId,
            CancellationToken cancellationToken = default)
        {
            var requestUri = $"/api/transactions?userId={userId}"; // Incluye el userId en la URI si es necesario
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