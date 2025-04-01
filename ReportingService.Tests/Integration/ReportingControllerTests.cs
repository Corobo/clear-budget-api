using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using ReportingService.Models.DTO;
using ReportingService.Tests.Fakes;
using Xunit;
using TransactionsService.Tests.Integration.Postgres;

namespace ReportingService.Tests.Integration
{
    public class ReportingControllerTests
    {
        private readonly HttpClient _client;

        public ReportingControllerTests()
        {
            _client = CreateAuthenticatedClient();
        }

        private static HttpClient CreateAuthenticatedClient()
        {
            var factory = new WebAppFactory();
            return factory.CreateClient();
            
        }

        [Fact]
        public async Task GetDashboard_ReturnsCorrectValues()
        {
            var response = await _client.GetAsync("/api/reporting/dashboard");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<DashboardReportDTO>();
            result.Should().NotBeNull();
            result!.TotalIncome.Should().Be(500);
            result.TotalExpenses.Should().Be(200);
            result.Balance.Should().Be(300);
        }

        [Fact]
        public async Task GetDashboard_ForceRefresh_StillReturnsCorrectData()
        {
            var response = await _client.GetAsync("/api/reporting/dashboard?force=true");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<DashboardReportDTO>();
            result.Should().NotBeNull();
            result!.Balance.Should().Be(300);
        }
    }
}
