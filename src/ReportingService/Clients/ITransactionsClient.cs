using ReportingService.Models.DTO;

namespace ReportingService.Clients
{
    public interface ITransactionsClient
    {
        Task<IEnumerable<TransactionDTO>> GetUserTransactionsAsync(Guid userId,
            CancellationToken cancellationToken = default);
    }
}
