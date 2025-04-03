using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using ReportingService.Clients;
using ReportingService.Models.DTO;

namespace ReportingService.Tests.Services
{
    public class ReportingServiceTests
    {
        private readonly Mock<ITransactionsClient> _transactionsClientMock;
        private readonly IMemoryCache _cache;
        private readonly ReportingService.Services.Impl.ReportingService _service;

        public ReportingServiceTests()
        {
            _transactionsClientMock = new Mock<ITransactionsClient>();

            _cache = new MemoryCache(new MemoryCacheOptions());
            _service = new ReportingService.Services.Impl.ReportingService(_transactionsClientMock.Object, _cache);
        }

        [Fact]
        public async Task GetDashboardReportAsync_ReturnsData_WhenNoCache()
        {
            // Arrange
            var userId = "user-1";
            var transactions = new List<TransactionDTO>
            {
                new() { Type = 1, Amount = 1000, Date = DateTime.UtcNow }, // income
                new() { Type = 0, Amount = 200, CategoryId = Guid.NewGuid(), Description = "Food", Date = DateTime.UtcNow } // expense
            };

            _transactionsClientMock
                .Setup(c => c.GetUserTransactionsAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactions);


            // Act
            var result = await _service.GetDashboardReportAsync(userId);

            // Assert
            result.TotalIncome.Should().Be(1000);
            result.TotalExpenses.Should().Be(200);
            result.Balance.Should().Be(800);
            result.ExpensesByCategory.Should().HaveCount(1);
            result.MonthlySummary.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetDashboardReportAsync_UsesCache_IfAvailable()
        {
            // Arrange
            var userId = "user-2";
            var report = new DashboardReportDTO { TotalIncome = 1234 };
            _cache.Set($"report:{userId}", report);
            var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            // Act
            var result = await _service.GetDashboardReportAsync(userId);

            // Assert
            result.Should().BeSameAs(report);
            _transactionsClientMock.Verify(c => c.GetUserTransactionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetDashboardReportAsync_ForceRefresh_BypassesCache()
        {
            // Arrange
            var userId = "user-3";
            var transactions = new List<TransactionDTO>
            {
                new() { Type = 1, Amount = 500, Date = DateTime.UtcNow }
            };

            _cache.Set($"report:{userId}", new DashboardReportDTO { TotalIncome = 999 });


            _transactionsClientMock
                .Setup(c => c.GetUserTransactionsAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactions);


            // Act
            var result = await _service.GetDashboardReportAsync(userId, forceRefresh: true);

            // Assert
            result.TotalIncome.Should().Be(500);
            result.Balance.Should().Be(500);
        }
    }
}
