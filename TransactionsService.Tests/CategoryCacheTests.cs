using FluentAssertions;
using Moq;
using TransactionsService.Clients;
using TransactionsService.Services.Impl;

namespace TransactionsService.Tests
{
    public class CategoryCacheTests
    {
        private readonly Mock<ICategoriesClient> _clientMock;
        private readonly CategoryCache _cache;

        public CategoryCacheTests()
        {
            _clientMock = new Mock<ICategoriesClient>();
            _cache = new CategoryCache(_clientMock.Object);
        }

        [Fact]
        public async Task InitializeAsync_Should_Retry_And_Load_Cache()
        {
            // Arrange
            var ids = new[] { Guid.NewGuid() };
            var callCount = 0;

            _clientMock.Setup(c => c.GetAllCategoryIdsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    if (callCount < 3)
                        throw new InvalidOperationException("Fake failure");
                    return ids;
                });

            // Act
            await _cache.InitializeAsync();

            // Assert
            callCount.Should().Be(3);
            _cache.IsValidCategory(ids[0]).Should().BeTrue();
        }

        [Fact]
        public async Task InitializeAsync_Should_Populate_Cache_When_Successful()
        {
            // Arrange
            var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };

            _clientMock.Setup(c => c.GetAllCategoryIdsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ids);

            // Act
            await _cache.InitializeAsync();

            // Assert
            foreach (var id in ids)
                _cache.IsValidCategory(id).Should().BeTrue();
        }

        [Fact]
        public async Task InitializeAsync_Should_Throw_If_All_Attempts_Fail()
        {
            // Arrange
            _clientMock.Setup(c => c.GetAllCategoryIdsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Always fails"));

            // Act
            Func<Task> action = async () => await _cache.InitializeAsync();

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
