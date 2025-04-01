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

        public async Task<IEnumerable<TransactionDTO>> GetUserTransactionsAsync(string userId)
        {
            var response = await _httpClient.GetAsync("/api/transactions");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<TransactionDTO>>()
                   ?? Enumerable.Empty<TransactionDTO>();
        }
    }


}