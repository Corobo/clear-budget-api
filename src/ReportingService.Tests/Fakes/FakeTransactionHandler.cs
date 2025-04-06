using ReportingService.Models.DTO;
using System.Net;
using System.Net.Http.Json;

namespace ReportingService.Tests.Fakes
{
    public class FakeTransactionsHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Get && request.RequestUri!.AbsolutePath == "/api/transactions")
            {
                var fakeTransactions = new List<TransactionDTO>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        CategoryId = Guid.NewGuid(),
                        Amount = 500,
                        Type = 1, // Income
                        Date = DateTime.UtcNow.AddDays(-1),
                        Description = "Salary"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        CategoryId = Guid.NewGuid(),
                        Amount = 200,
                        Type = 0, // Expense
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

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}
