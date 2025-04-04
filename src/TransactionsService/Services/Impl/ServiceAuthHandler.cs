using System.Net.Http.Headers;
using TransactionsService.Clients;

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
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await base.SendAsync(request, cancellationToken);
        }
    }

}
