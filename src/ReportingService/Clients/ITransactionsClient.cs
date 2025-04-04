using ReportingService.Models.DTO;

namespace ReportingService.Clients
{
    public interface ITransactionsClient
    {
        Task<IEnumerable<TransactionDTO>> GetUserTransactionsAsync(string userId,
            CancellationToken cancellationToken = default);
    }
}
