using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Shared.Testing.Containers;
using System.Net;
using System.Net.Http.Json;
using TransactionsService.Models.DTO;
using TransactionsService.Tests.Integration.Fixtures;
using TransactionsService.Tests.Integration.Factories;
using Shared.Testing.Settings;

namespace TransactionsService.Tests.Integration
{
    public class IntegrationTests : IClassFixture<TransactionsDbFixture>, IClassFixture<RabbitMqContainerFixture>
    {

        private readonly HttpClient _client;
        private readonly RabbitMqContainerFixture _rabbitFixture;
        private readonly TransactionsDbFixture _postgresFixture;

        public IntegrationTests(TransactionsDbFixture fixture, RabbitMqContainerFixture rabbitMqContainerFixture)
        {
            _client = CreateAuthenticatedClient(fixture, rabbitMqContainerFixture);
            _rabbitFixture = rabbitMqContainerFixture;
            _postgresFixture = fixture;
        }

        private static HttpClient CreateAuthenticatedClient(TransactionsDbFixture fixture, RabbitMqContainerFixture rabbitMqContainerFixture)
        {
            var factory = new TransactionsWebAppFactory(fixture.ConnectionString, RabbitTestSettings.UseRabbitSettings(rabbitMqContainerFixture));
            return factory.CreateClient();
        }

        [Fact]
        public async Task GetAllTransactions_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/transactions");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateTransaction_ReturnsCreated()
        {
            var body = JsonContent.Create(new
            {
                categoryId = Guid.NewGuid(), // puede venir de seed
                amount = 55.75m,
                description = "Test transaction",
                type = 0, // Expense
                date = DateTime.UtcNow
            });

            var response = await _client.PostAsync("/api/transactions", body);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task UpdateTransaction_ReturnsNoContent()
        {
            var transactionId = await CreateTestTransaction();

            var body = JsonContent.Create(new { description = "Updated description" });
            var response = await _client.PutAsync($"/api/transactions/{transactionId}", body);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteTransaction_ReturnsNoContent()
        {
            var transactionId = await CreateTestTransaction();

            var response = await _client.DeleteAsync($"/api/transactions/{transactionId}");
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        private async Task<Guid> CreateTestTransaction()
        {
            var body = JsonContent.Create(new
            {
                categoryId = Guid.NewGuid(), // puede venir de seed
                amount = 10.00m,
                description = $"Temp-{Guid.NewGuid()}",
                type = 1, // Income
                date = DateTime.UtcNow
            });

            var response = await _client.PostAsync("/api/transactions", body);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<TransactionDTO>();
            return created!.Id;
        }
    }
}
