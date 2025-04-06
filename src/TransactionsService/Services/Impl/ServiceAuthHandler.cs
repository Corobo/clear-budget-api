using System.Net.Http.Headers;
using TransactionsService.Clients;
using Serilog;

namespace TransactionsService.Services.Impl
{
    public class ServiceAuthHandler : DelegatingHandler
    {
        private readonly IAuthTokenClient _tokenService;

        public ServiceAuthHandler(IAuthTokenClient tokenClient)
        {
            _tokenService = tokenClient;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _tokenService.GetAccessTokenAsync();
            Log.Information("Adding Bearer token to request: {Token}", token);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await base.SendAsync(request, cancellationToken);
        }
    }

}
