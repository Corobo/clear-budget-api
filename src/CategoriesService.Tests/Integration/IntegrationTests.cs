using CategoriesService.Models.DTO;
using CategoriesService.Tests.Integration.Factories;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Shared.Testing.Containers;
using Shared.Testing.Settings;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using CategoriesService.Tests.Integration.Fixtures;
using Xunit;

namespace CategoriesService.Tests.Integration
{
    public class IntegrationTests : IClassFixture<CategoriesDbFixture>, IClassFixture<RabbitMqContainerFixture>
    {

        protected readonly HttpClient _client;
        private readonly RabbitMqContainerFixture _rabbitFixture;


        public IntegrationTests(CategoriesDbFixture fixture, RabbitMqContainerFixture rabbitFixture)
        {
            _client = CreateAuthenticatedClient(fixture, rabbitFixture);
            _rabbitFixture = rabbitFixture;
        }

        private static HttpClient CreateAuthenticatedClient(CategoriesDbFixture fixture, RabbitMqContainerFixture rabbitMqContainerFixture)
        {
            var factory = new CategoriesWebAppFactory(fixture.ConnectionString,RabbitTestSettings.UseRabbitSettings(rabbitMqContainerFixture));
            return factory.CreateClient();
        }

        [Fact]
        public async Task GetMergedCategories_Returns200()
        {
            var response = await _client.GetAsync("/api/categories?userId=00000000-0000-0000-0000-000000000001");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateUserCategory_ReturnsCreated()
        {
            var body = JsonContent.Create(new { name = "TestCat", color = "#abc123" });
            var response = await _client.PostAsync("/api/categories", body);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task UpdateCategory_ReturnsNoContent()
        {
            // Replace with seeded ID or dynamic insert
            var categoryId = await CreateTestCategory();

            var body = JsonContent.Create(new { color = "#000000" });
            var response = await _client.PutAsync($"/api/categories/{categoryId}", body);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteUserCategory_ReturnsNoContent()
        {
            var categoryId = await CreateTestCategory();

            var response = await _client.DeleteAsync($"/api/categories/{categoryId}");
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task CreateUserCategory_Should_Publish_CategoryCreated_Event()
        {
            // Arrange
            var factory = new CategoriesWebAppFactory("", RabbitTestSettings.UseRabbitSettings(_rabbitFixture));

            var client = factory.CreateClient();

            var userId = "00000000-0000-0000-0000-000000000001";
            var request = new CategoryCreateDTO
            {
                Name = "Test from Integration",
                Color = "#009900"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/categories", request);
            response.EnsureSuccessStatusCode();

            // Assert
            using var channel = await _rabbitFixture.GetChannelAsync().ConfigureAwait(false);

            string? message = null;
            var timeout = DateTime.UtcNow.AddSeconds(5);

            while (DateTime.UtcNow < timeout)
            {
                var result = await channel.BasicGetAsync(_rabbitFixture.QueueName, autoAck: true);

                if (result != null)
                {
                    message = Encoding.UTF8.GetString(result.Body.ToArray());
                    break;
                }

                await Task.Delay(500);
            }

            message.Should().NotBeNullOrEmpty("Expected a message in the queue but got none.");

            var payload = JObject.Parse(message);
            payload["EventType"].ToString().Should().Be("category.created");
            payload["UserId"].ToObject<Guid>().Should().Be(Guid.Parse(userId));
        }

        protected async Task<Guid> CreateTestCategory()
        {
            var body = JsonContent.Create(new { name = $"Temp-{Guid.NewGuid()}", color = "#333333" });
            var response = await _client.PostAsync("/api/categories", body);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<CategoryDTO>();
            return created!.Id;
        }
    }


}
