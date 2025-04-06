using TransactionsService.Models.DTO;

namespace TransactionsService.Services
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionDTO>> GetAllByUserIdAsync(Guid userId);
        Task<TransactionDTO?> GetByIdAsync(Guid id, Guid userId);
        Task<TransactionDTO> CreateAsync(CreateTransactionDTO dto, Guid userId);
        Task<bool> UpdateAsync(Guid id, UpdateTransactionDTO dto, Guid userId);
        Task<bool> DeleteAsync(Guid id, Guid userId);
    }
}
