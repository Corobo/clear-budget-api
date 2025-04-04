namespace Shared.Messaging.Configuration
{
    public sealed class RabbitMQOptions
    {
        public const string ConfigurationSectionName = "RabbitMQ";

        public string? Host { get; set; }
        public string? VirtualHost { get; set; }
        public int? Port { get; set; }
        public string? User { get; set; }
        public string? Password { get; set; }

        public Dictionary<string, string>? ExchangeNames { get; set; }
        public Dictionary<string, string>? QueueNames { get; set; }

        public RabbitMQSSLOptions? Ssl { get; set; }
    }
}
