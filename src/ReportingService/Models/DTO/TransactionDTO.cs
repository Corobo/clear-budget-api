namespace ReportingService.Models.DTO
{
    public class TransactionDTO
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Type { get; set; } // 0 = Expense, 1 = Income
        public DateTime Date { get; set; }
    }
}
