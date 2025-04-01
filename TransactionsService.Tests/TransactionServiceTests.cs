using AutoMapper;
using Moq;
using TransactionsService.Models.DB;
using TransactionsService.Models.DTO;
using TransactionsService.Models.Enums;
using TransactionsService.Repositories;
using TransactionsService.Services.Impl;
using Xunit;

namespace TransactionsService.Tests.Services
{
    public class TransactionServiceTests
    {
        private readonly Mock<ITransactionRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly TransactionService _service;

        public TransactionServiceTests()
        {
            _repositoryMock = new Mock<ITransactionRepository>();
            _mapperMock = new Mock<IMapper>();
            _service = new TransactionService(_repositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetAllByUserIdAsync_ReturnsMappedTransactions()
        {
            // Arrange
            var userId = "user-123";
            var transactions = new List<Transaction> { new() { Id = Guid.NewGuid(), UserId = userId } };
            var dtos = new List<TransactionDTO> { new() { Id = transactions[0].Id } };

            _repositoryMock.Setup(r => r.GetAllByUserIdAsync(userId))
                .ReturnsAsync(transactions);
            _mapperMock.Setup(m => m.Map<IEnumerable<TransactionDTO>>(transactions))
                .Returns(dtos);

            // Act
            var result = await _service.GetAllByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(transactions[0].Id, result.First().Id);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsTransactionDTO_WhenExists()
        {
            var id = Guid.NewGuid();
            var userId = "user-456";
            var transaction = new Transaction { Id = id, UserId = userId };
            var dto = new TransactionDTO { Id = id };

            _repositoryMock.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync(transaction);
            _mapperMock.Setup(m => m.Map<TransactionDTO>(transaction))
                .Returns(dto);

            var result = await _service.GetByIdAsync(id, userId);

            Assert.NotNull(result);
            Assert.Equal(id, result?.Id);
        }

        [Fact]
        public async Task CreateAsync_MapsAndCreatesTransaction()
        {
            var userId = "user-789";
            var createDto = new CreateTransactionDTO
            {
                Amount = 123,
                CategoryId = Guid.NewGuid(),
                Description = "Test",
                Type = TransactionType.Expense,
                Date = DateTime.UtcNow
            };
            var entity = new Transaction { Id = Guid.NewGuid(), UserId = userId };
            var dto = new TransactionDTO { Id = entity.Id };

            _mapperMock.Setup(m => m.Map<Transaction>(createDto))
                .Returns(entity);
            _repositoryMock.Setup(r => r.CreateAsync(entity))
                .ReturnsAsync(entity);
            _mapperMock.Setup(m => m.Map<TransactionDTO>(entity))
                .Returns(dto);

            var result = await _service.CreateAsync(createDto, userId);

            Assert.Equal(dto.Id, result.Id);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsFalse_WhenTransactionNotFound()
        {
            var id = Guid.NewGuid();
            var userId = "user-abc";
            var updateDto = new UpdateTransactionDTO { Amount = 99 };

            _repositoryMock.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync((Transaction?)null);

            var result = await _service.UpdateAsync(id, updateDto, userId);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesTransaction_WhenExists()
        {
            var id = Guid.NewGuid();
            var userId = "user-def";
            var updateDto = new UpdateTransactionDTO { Description = "Updated" };
            var entity = new Transaction { Id = id, UserId = userId, Description = "Old" };

            _repositoryMock.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync(entity);
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Transaction>()))
                .ReturnsAsync(true);

            _mapperMock.Setup(m => m.Map(updateDto, entity));

            var result = await _service.UpdateAsync(id, updateDto, userId);

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_DelegatesToRepository()
        {
            var id = Guid.NewGuid();
            var userId = "user-xyz";

            _repositoryMock.Setup(r => r.DeleteAsync(id, userId))
                .ReturnsAsync(true);

            var result = await _service.DeleteAsync(id, userId);

            Assert.True(result);
        }
    }
}
