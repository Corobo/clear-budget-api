namespace ReportingService.Models.DTO
{
    public class DashboardReportDTO
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal Balance => TotalIncome - TotalExpenses;

        public List<CategorySummaryDTO> ExpensesByCategory { get; set; } = new();
        public List<MonthlySummaryDTO> MonthlySummary { get; set; } = new();
    }
}
