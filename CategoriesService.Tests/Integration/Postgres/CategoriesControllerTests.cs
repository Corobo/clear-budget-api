using CategoriesService.Models.DTO;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CategoriesService.Tests.Integration.Postgres;

public class CategoriesControllerTests : IClassFixture<PostgreSqlContainerFixture>
{

    protected readonly HttpClient _client;


    public CategoriesControllerTests(PostgreSqlContainerFixture fixture)
    {
        _client = CreateAuthenticatedClient(fixture);
    }

    private static HttpClient CreateAuthenticatedClient(PostgreSqlContainerFixture fixture)
    {
        var factory = new WebAppFactory(fixture.ConnectionString);


        return factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", fixture.ConnectionString);
            builder.ConfigureServices(services =>
            {
                services.PostConfigureAll<AuthenticationOptions>(opts =>
                {
                    opts.DefaultAuthenticateScheme = "Test";
                    opts.DefaultChallengeScheme = "Test";
                });
            });
        }).CreateClient();
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
        var response = await _client.PostAsync("/api/categories/user", body);
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

        var response = await _client.DeleteAsync($"/api/categories/user/{categoryId}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    protected async Task<Guid> CreateTestCategory()
    {
        var body = JsonContent.Create(new { name = $"Temp-{Guid.NewGuid()}", color = "#333333" });
        var response = await _client.PostAsync("/api/categories/user", body);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<CategoryDTO>();
        return created!.Id;
    }
}
