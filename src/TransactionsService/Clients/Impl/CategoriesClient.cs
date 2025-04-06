using System.Text.Json;

namespace TransactionsService.Clients.Impl
{
    public class CategoriesClient : ICategoriesClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public CategoriesClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _config = configuration;
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

        public async Task<string> GetAccessTokenAsync()
        {
            var clientId = _config["Auth:ClientId"];
            var clientSecret = _config["Auth:ClientSecret"];
            var tokenUrl = _config["Auth:TokenUrl"];

            var body = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
        });

            var response = await _httpClient.PostAsync(tokenUrl, body);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("access_token").GetString();
        }
    }
}