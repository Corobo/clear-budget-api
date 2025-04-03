using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Testcontainers.RabbitMq;

namespace TransactionsService.Tests.Integration.RabbitMQ
{
    public class RabbitMqContainerFixture : IAsyncLifetime
    {
        public RabbitMqContainer Container { get; private set; } = null!;
        public string Hostname => "localhost";
        public string Username => "testuser";
        public string Password => "testpass";
        public int Port => 5672; // Local binding
        public string QueueName => "transactions.category.queue";

        public string ConnectionString =>
            $"amqp://{Username}:{Password}@{Hostname}:{Port}/";

        public async Task InitializeAsync()
        {
            Container = new RabbitMqBuilder()
                .WithUsername(Username)
                .WithPassword(Password)
                .WithPortBinding(5672, 5672)     // Broker (AMQP)
                .WithPortBinding(15672, 15672)   // Management UI
                .WithImage("rabbitmq:3-management")
                .WithCleanUp(true)
                .Build();

            await Container.StartAsync();
            await InitializeCategory();
        }

        public async Task DisposeAsync()
        {
            await Container.StopAsync();
            await Task.Delay(TimeSpan.FromSeconds(15));
        }

        public async Task PublishAsync<T>(T message, string exchange, string routingKey = "")
        {
            using var channel = await GetChannelAsync();

            await channel.ExchangeDeclareAsync(exchange, ExchangeType.Fanout, durable: true);
            await channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false);
            await channel.QueueBindAsync(QueueName, exchange: exchange, routingKey: "");

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent
            };

            await Task.Run(() => channel.BasicPublishAsync(
                exchange,
                routingKey,
                false,
                props,
                body));
        }

        private async Task<IChannel> GetChannelAsync()
        {
            var factory = new ConnectionFactory
            {
                HostName = Hostname,
                Port = Port,
                UserName = Username,
                Password = Password
            };
            var connection = await factory.CreateConnectionAsync();
            return await connection.CreateChannelAsync();
        }

        private async Task InitializeCategory()
        {
            using var channel = await GetChannelAsync();

            await channel.ExchangeDeclareAsync("category.exchange", ExchangeType.Fanout, durable: true);
            await channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false);
            await channel.QueueBindAsync(QueueName, exchange: "category.exchange", routingKey: "");
        }


    }
}
