using TransactionsService.Models.DB;

namespace TransactionsService.Repositories
{

    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> GetAllByUserIdAsync(Guid userId);
        Task<Transaction?> GetByIdAsync(Guid id, Guid userId);
        Task<Transaction> CreateAsync(Transaction transactionDTO);
        Task<bool> UpdateAsync(Transaction transactionDTO);
        Task<bool> DeleteAsync(Guid id, Guid userId);
    }
}
