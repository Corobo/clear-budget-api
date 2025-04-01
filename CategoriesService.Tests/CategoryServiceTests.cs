using AutoMapper;
using CategoriesService.Models.DB;
using CategoriesService.Models.DTO;
using CategoriesService.Repositories;
using CategoriesService.Services;
using CategoriesService.Services.Impl;
using FluentAssertions;
using Moq;
using Org.BouncyCastle.Crypto;
using Xunit;

namespace CategoriesService.Tests;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly ICategoryService _service;

    public CategoryServiceTests()
    {
        _service = new CategoryService(_repoMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetAdminCategoriesAsync_ShouldReturnMappedDtos()
    {
        // Arrange
        var categories = new List<Category>
        {
            new() { Id = Guid.NewGuid(), Name = "Food", Color = "#ff0000" }
        };

        var categoryDtos = new List<CategoryDTO>
        {
            new(categories[0].Id, "Food", "#ff0000")
        };

        _repoMock.Setup(r => r.GetAdminCategoriesAsync())
            .ReturnsAsync(categories);

        _mapperMock.Setup(m => m.Map<CategoryDTO>(It.IsAny<Category>()))
            .Returns<Category>(c => new CategoryDTO(c.Id, c.Name, c.Color));

        // Act
        var result = await _service.GetAdminCategoriesAsync();

        // Assert
        result.Should().BeEquivalentTo(categoryDtos);
    }
}
