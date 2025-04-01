using TransactionsService.Models.Enums;

namespace TransactionsService.Models.DTO
{
    public class CreateTransactionDTO
    {
        public Guid CategoryId { get; set; }

        public decimal Amount { get; set; }

        public string Description { get; set; } = string.Empty;

        public TransactionType Type { get; set; }

        public DateTime Date { get; set; }
    }
}
