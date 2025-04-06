using TransactionsService.Models.Enums;

namespace TransactionsService.Models.DTO
{
    public class UpdateTransactionDTO
    {
        public decimal? Amount { get; set; }

        public string? Description { get; set; }

        public Guid? CategoryId { get; set; }

        public TransactionType? Type { get; set; }

        public DateTime? Date { get; set; }
    }
}
