using TransactionsService.Models.Enums;

namespace TransactionsService.Models.DB
{
    public class Transaction
    {
        public Guid Id { get; set; }

        // User info from JWT
        public string UserId { get; set; } = null!;

        // Category association
        public Guid CategoryId { get; set; }

        public decimal Amount { get; set; }

        public string Description { get; set; } = string.Empty;

        public TransactionType Type { get; set; } // Income or Expense

        public DateTime Date { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
