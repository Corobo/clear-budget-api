using AutoMapper;
using CategoriesService.Models.DB;
using CategoriesService.Models.DTO;
using CategoriesService.Repositories;
using CategoriesService.Services;
using CategoriesService.Services.Impl;
using FluentAssertions;
using Messaging.Configuration;
using Messaging.EventBus;
using Messaging.Events;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CategoriesService.Tests
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _repoMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IEventBusProducer<CategoryEvent>> _eventBusMock = new();
        private readonly Mock<IOptions<RabbitMQOptions>> _optionsMock = new();

        private readonly ICategoryService _service;

        public CategoryServiceTests()
        {
            _optionsMock.Setup(o => o.Value).Returns(new RabbitMQOptions
            {
                ExchangeNames = new Dictionary<string, string>
                {
                    { "Category", "category.exchange" }
                }
            });

            _service = new CategoryService(
                _repoMock.Object,
                _mapperMock.Object,
                _eventBusMock.Object,
                _optionsMock.Object
            );
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

        [Fact]
        public async Task CreateUserCategoryAsync_Should_Publish_Event()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new CategoryCreateDTO { Name = "Gym", Color = "#123456" };
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Color = dto.Color,
                UserId = userId
            };

            _mapperMock.Setup(m => m.Map<Category>(dto)).Returns(category);
            _mapperMock.Setup(m => m.Map<CategoryDTO>(It.IsAny<Category>()))
                .Returns<Category>(c => new CategoryDTO(c.Id, c.Name, c.Color));

            _repoMock.Setup(r => r.AddAsync(category)).Returns(Task.FromResult(1));
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.FromResult(1));

            CategoryEvent? capturedEvent = null;

            _eventBusMock
                .Setup(p => p.PublishAsync(It.IsAny<CategoryEvent>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<CategoryEvent, string, string>((e, _, _) => capturedEvent = e);

            // Act
            var result = await _service.CreateUserCategoryAsync(userId, dto);

            // Assert
            capturedEvent.Should().NotBeNull();
            capturedEvent!.EventType.Should().Be("category.created");
            capturedEvent.UserId.Should().Be(userId);
            capturedEvent.CategoryId.Should().NotBeEmpty();

        }
    }
}
