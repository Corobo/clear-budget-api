using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ReportingService.Tests.Fakes
{
    public class FakeServiceAuthHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "fake-token");
            return base.SendAsync(request, cancellationToken);
        }
    }

}
