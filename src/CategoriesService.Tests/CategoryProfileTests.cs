using AutoMapper;
using CategoriesService.Models.DB;
using CategoriesService.Models.DTO;
using CategoriesService.Profiles;
using FluentAssertions;
using Xunit;

namespace CategoriesService.Tests
{
    public class CategoryProfileTests
    {
        private readonly IMapper _mapper;

        public CategoryProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CategoryProfile>();
            });

            _mapper = config.CreateMapper();
        }

        [Fact]
        public void Map_ShouldSetIsAdminTrue_WhenUserIdIsNull()
        {
            // Arrange
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "General",
                Color = "#FFFFFF",
                UserId = null
            };

            // Act
            var dto = _mapper.Map<CategoryDTO>(category);

            // Assert
            dto.IsAdmin.Should().BeTrue();
        }

        [Fact]
        public void Map_ShouldSetIsAdminFalse_WhenUserIdIsNotNull()
        {
            // Arrange
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Personal",
                Color = "#000000",
                UserId = Guid.NewGuid()
            };

            // Act
            var dto = _mapper.Map<CategoryDTO>(category);

            // Assert
            dto.IsAdmin.Should().BeFalse();
        }
    }
}
