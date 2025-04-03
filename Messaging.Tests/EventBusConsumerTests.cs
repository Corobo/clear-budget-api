using FluentAssertions;
using Messaging.EventBus.Impl;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System.Text;

namespace Messaging.Tests
{
    public class EventBusConsumerTests
    {
        public class DummyEvent
        {
            public string Data { get; set; } = default!;
        }

        private class DummyEventConsumer : EventBusConsumer<DummyEvent>
        {
            public bool WasCalled { get; private set; } = false;

            public DummyEventConsumer(ILogger logger) : base(logger) { }

            protected override Task HandleAsync(DummyEvent context)
            {
                WasCalled = true;
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task Consume_Should_Call_HandleAsync_With_Valid_Event()
        {
            var loggerMock = new Mock<ILogger>();
            var consumer = new DummyEventConsumer(loggerMock.Object);

            var eventArgs = CreateBasicDeliverEventArgs();

            var context = new DummyEvent { Data = "Hello" };
            await consumer.ConsumeAsync(eventArgs, context);

            consumer.WasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Consume_Should_Call_HandleAsync_When_Context_Is_Valid()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var consumer = new DummyEventConsumer(loggerMock.Object);

            var context = new DummyEvent { Data = "Test" };
            var @event = CreateBasicDeliverEventArgs();

            // Act
            await consumer.ConsumeAsync(@event, context);

            // Assert
            consumer.WasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Consume_Should_Not_Throw_When_Context_Is_Null()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var consumer = new DummyEventConsumer(loggerMock.Object);
            var @event = CreateBasicDeliverEventArgs();

            // Act
            var act = async () => await consumer.ConsumeAsync(@event, null);

            // Assert
            await act.Should().NotThrowAsync();
            consumer.WasCalled.Should().BeFalse();
        }

        [Fact]
        public async Task Consume_Should_Throw_If_HandleAsync_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            var consumer = new FailingConsumer(loggerMock.Object);
            var context = new DummyEvent { Data = "Fail" };
            var @event = CreateBasicDeliverEventArgs();

            // Act
            Func<Task> act = async () => await consumer.ConsumeAsync(@event, context);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        private class FailingConsumer : EventBusConsumer<DummyEvent>
        {
            public FailingConsumer(ILogger logger) : base(logger) { }

            protected override Task HandleAsync(DummyEvent context)
            {
                throw new InvalidOperationException("Failure in HandleAsync");
            }
        }

        private BasicDeliverEventArgs CreateBasicDeliverEventArgs()
        {
            var json = "{ \"data\": \"test\" }";
            var body = Encoding.UTF8.GetBytes(json);
            var memoryBody = new ReadOnlyMemory<byte>(body);

            var props = new BasicProperties
            {
                ContentType = "application/json"
            };

            var eventArgs = new BasicDeliverEventArgs(
                consumerTag: "test-consumer",
                deliveryTag: 1,
                redelivered: false,
                exchange: "test.exchange",
                routingKey: "test.routing.key",
                properties: props,
                body: memoryBody
            );

            return eventArgs;
        }
    }
}
