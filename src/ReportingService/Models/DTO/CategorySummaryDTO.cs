namespace ReportingService.Models.DTO
{
    public class CategorySummaryDTO
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }
}
