using TransactionsService.Models.DB;
using TransactionsService.Models.DTO;

namespace TransactionsService.Repositories
{

    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> GetAllByUserIdAsync(string userId);
        Task<Transaction?> GetByIdAsync(Guid id, string userId);
        Task<Transaction> CreateAsync(Transaction transactionDTO);
        Task<bool> UpdateAsync(Transaction transactionDTO);
        Task<bool> DeleteAsync(Guid id, string userId);
    }
}
