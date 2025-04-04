using Shared.Messaging.Events;
using Moq;
using RabbitMQ.Client.Events;
using Serilog;
using System.Text;
using TransactionsService.Messaging.Consumers;
using TransactionsService.Services;

namespace TransactionsService.Tests
{
    public class CategoryEventConsumerTests
    {
        [Fact]
        public async Task Consume_Should_Invoke_RefreshAsync()
        {
            // Arrange
            var cacheMock = new Mock<ICategoryCache>();
            var loggerMock = new Mock<ILogger>();

            var consumer = new CategoryEventConsumer(cacheMock.Object, loggerMock.Object);

            var context = new CategoryEvent
            {
                EventType = "category.updated",
                CategoryId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            var eventArgs = BuildDummyEventArgs();

            // Act
            await consumer.ConsumeAsync(eventArgs, context);

            // Assert
            cacheMock.Verify(c => c.RefreshAsync(), Times.Once);
        }

        private BasicDeliverEventArgs BuildDummyEventArgs()
        {
            var body = Encoding.UTF8.GetBytes("{ \"dummy\": true }");

            return new BasicDeliverEventArgs(
                consumerTag: "test",
                deliveryTag: 1,
                redelivered: false,
                exchange: "test.exchange",
                routingKey: "test.key",
                properties: new RabbitMQ.Client.BasicProperties(),
                body: new ReadOnlyMemory<byte>(body)
            );
        }
    }
}
