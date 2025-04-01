using Microsoft.Extensions.Caching.Memory;
using ReportingService.Clients;
using ReportingService.Models.DTO;

namespace ReportingService.Services.Impl
{
    public class ReportingService : IReportingService
    {
        private readonly ITransactionsClient _transactionsClient;
        private readonly IMemoryCache _cache;

        public ReportingService(ITransactionsClient transactionsClient, IMemoryCache cache)
        {
            _transactionsClient = transactionsClient;
            _cache = cache;
        }

        public async Task<DashboardReportDTO> GetDashboardReportAsync(string userId, bool forceRefresh = false)
        {
            var cacheKey = $"report:{userId}";

            if (!forceRefresh && _cache.TryGetValue(cacheKey, out DashboardReportDTO cachedReport))
            {
                return cachedReport;
            }

            var transactions = await _transactionsClient.GetUserTransactionsAsync(userId);

            var income = transactions.Where(t => t.Type == 1).Sum(t => t.Amount);
            var expenses = transactions.Where(t => t.Type == 0).Sum(t => t.Amount);

            var byCategory = transactions
                .Where(t => t.Type == 0)
                .GroupBy(t => t.CategoryId)
                .Select(g => new CategorySummaryDTO
                {
                    CategoryId = g.Key,
                    CategoryName = g.First().Description,
                    Total = g.Sum(t => t.Amount)
                }).ToList();

            var monthly = transactions
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .Select(g => new MonthlySummaryDTO
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Income = g.Where(t => t.Type == 1).Sum(t => t.Amount),
                    Expenses = g.Where(t => t.Type == 0).Sum(t => t.Amount)
                }).OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ToList();

            var report = new DashboardReportDTO
            {
                TotalIncome = income,
                TotalExpenses = expenses,
                ExpensesByCategory = byCategory,
                MonthlySummary = monthly
            };

            _cache.Set(cacheKey, report, TimeSpan.FromMinutes(30));

            return report;
        }
    }
}
