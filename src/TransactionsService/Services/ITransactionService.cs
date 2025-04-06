using TransactionsService.Models.DTO;

namespace TransactionsService.Services
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionDTO>> GetAllByUserIdAsync(string userId);
        Task<TransactionDTO?> GetByIdAsync(Guid id, string userId);
        Task<TransactionDTO> CreateAsync(CreateTransactionDTO dto, string userId);
        Task<bool> UpdateAsync(Guid id, UpdateTransactionDTO dto, string userId);
        Task<bool> DeleteAsync(Guid id, string userId);
    }
}
