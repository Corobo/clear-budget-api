using ReportingService.Models.DTO;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace ReportingService.Tests.Fakes
{
    public class FakeTransactionsHandler : HttpMessageHandler
    {
        private static readonly Regex _transactionsByUserRegex = new Regex(@"^/api/transactions/by-user/(?<userId>[a-fA-F0-9\-]+)$");

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Get && request.RequestUri != null)
            {
                var match = _transactionsByUserRegex.Match(request.RequestUri.AbsolutePath);

                if (match.Success)
                {
                    var userId = Guid.Parse(match.Groups["userId"].Value); // úsalo si quieres condicionar la respuesta

                    var fakeTransactions = new List<TransactionDTO>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CategoryId = Guid.NewGuid(),
                            Amount = 500,
                            Type = 1,
                            Date = DateTime.UtcNow.AddDays(-1),
                            Description = "Salary"
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            CategoryId = Guid.NewGuid(),
                            Amount = 200,
                            Type = 0,
                            Date = DateTime.UtcNow.AddDays(-2),
                            Description = "Groceries"
                        }
                    };

                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(fakeTransactions)
                    };

                    return Task.FromResult(response);
                }
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}
