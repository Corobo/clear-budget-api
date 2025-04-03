using Messaging.Configuration;
using Messaging.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Messaging.Background
{
    public abstract class SubscriptionService<T> : BackgroundService where T : class
    {
        protected string QueueName;

        private readonly ILogger _logger;
        private IEventBusConsumer<T> _consumer;
        protected readonly IServiceScopeFactory _scopeFactory;
        private IConnection? _connection;
        private IChannel? _channel;
        public readonly IOptions<RabbitMQOptions> _options;

        public SubscriptionService(ILogger logger, IOptions<RabbitMQOptions> options, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _options = options;
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();

            base.Dispose();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _connection = await CreateRabbitMqConnection();
                _channel = await _connection.CreateChannelAsync();

                await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false).ConfigureAwait(false);

                AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += OnMessageReceived;


                await _channel.BasicConsumeAsync(QueueName, false, consumer).ConfigureAwait(false);
                _logger.Information("Subscribed to RabbitMQ messages on queue {queueName}", QueueName);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.Error("Not able to subscribe to RabbitMQ messages on queue {queueName}, exception {exception}", QueueName, ex.Message);
                await Task.CompletedTask;
            }
        }

        private async Task<IConnection> CreateRabbitMqConnection()
        {
            ConnectionFactory connectionFactory = new ConnectionFactory
            {
                HostName = _options.Value.Host,
                UserName = _options.Value.User,
                Password = _options.Value.Password,
            };

            if (_options.Value.Port.HasValue)
                connectionFactory.Port = _options.Value.Port.Value;

            if (_options.Value.VirtualHost != null)
                connectionFactory.VirtualHost = _options.Value.VirtualHost;

            string? rootCertificatePath = null;
            RabbitMQSSLOptions? sslOptions = _options.Value.Ssl;
            if (sslOptions != null)
            {
                connectionFactory.Ssl.Enabled = sslOptions.Enabled;
                connectionFactory.Ssl.ServerName = connectionFactory.HostName;

                if (sslOptions.Protocols != null)
                    connectionFactory.AmqpUriSslProtocols = sslOptions.Protocols.Value;

                RabbitMQCertificateOptions? keyStoreOptions = sslOptions.KeyStore;
                if (keyStoreOptions != null)
                {
                    connectionFactory.Ssl.CertPath = keyStoreOptions.Path;
                    connectionFactory.Ssl.CertPassphrase = keyStoreOptions.Password;
                }

                RabbitMQCertificateOptions? trustStoreOptions = sslOptions.TrustStore;
                if (trustStoreOptions != null)
                {
                    rootCertificatePath = trustStoreOptions.Path;
                    if (rootCertificatePath != null)
                        connectionFactory.Ssl.CertificateValidationCallback += (_, certificate, chain, sslPolicyErrors) => ValidatePeerCertificate(certificate, chain, sslPolicyErrors, rootCertificatePath, trustStoreOptions.Password);
                }
            }

            _logger.Information("[RabbitMQ] HostName: {hostName}", connectionFactory.HostName);
            _logger.Information("[RabbitMQ] UserName: {userName}", connectionFactory.UserName);
            _logger.Information("[RabbitMQ] VirtualHost: {virtualHost}", connectionFactory.VirtualHost);
            _logger.Information("[RabbitMQ] Port: {port}", connectionFactory.Port);
            _logger.Information("[RabbitMQ] SslServerName: {sslServerName}", connectionFactory.Ssl.ServerName);
            _logger.Information("[RabbitMQ] SslEnabled: {sslEnabled}", connectionFactory.Ssl.Enabled);
            _logger.Information("[RabbitMQ] SslAlgorithm: {sslAlgorithm}", connectionFactory.AmqpUriSslProtocols);
            _logger.Information("[RabbitMQ] SslCertPath: {sslCertPath}", connectionFactory.Ssl.CertPath);
            _logger.Information("[RabbitMQ] RootCertificatePath: {sslRootCertificatePath}", rootCertificatePath);

            _logger.Information("Creating RabbitMQ connection - Host: {host} VirtualHost: {virtualHost} Port: {port} User: {user}", connectionFactory.HostName, connectionFactory.VirtualHost, connectionFactory.Port, connectionFactory.UserName);

            return await connectionFactory.CreateConnectionAsync();
        }

        // Inspired by npgsql
        private bool ValidatePeerCertificate(X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors, string certificatePath, string? certificatePassword)
        {
            _logger.Information("Validating RabbitMQ root certificate");
            _logger.Information("Issuer {certificateIssuer}", certificate?.Issuer);
            _logger.Information("Subject {certificateSubject}", certificate?.Subject);
            _logger.Information("Certificate chain status {certificateChainStatus}", chain != null ? string.Join(Environment.NewLine, chain.ChainStatus.Select(x => $"{x.Status} ({x.StatusInformation})")) : null);
            _logger.Information("Certificate errors {certificateErrors}", sslPolicyErrors);

            if (certificate is null || chain is null)
            {
                _logger.Error("Certificate or chain is null");
                return false;
            }

            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                _logger.Information("No SSL issues");
                return true;
            }

            if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch))
            {
                _logger.Error("SSL error {sslError}", nameof(SslPolicyErrors.RemoteCertificateNameMismatch));
                return false;
            }

            X509Certificate2Collection certs = new X509Certificate2Collection();

            if (Path.GetExtension(certificatePath).ToUpperInvariant() != ".PFX")
            {
                _logger.Information("Importing root certificate from PEM file {certificateRootPath}", certificatePath);
                certs.ImportFromPemFile(certificatePath);
            }

            if (certs.Count == 0)
            {
                _logger.Information("Adding root certificate");
                certs.Add(new X509Certificate2(certificatePath, certificatePassword));
            }
            else
            {
                _logger.Information("Did not add root certificate");
            }

            chain.ChainPolicy.CustomTrustStore.AddRange(certs);
            chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;

            chain.ChainPolicy.ExtraStore.AddRange(certs);

            _logger.Information("Building certificate chain from certificate {certificateType}", certificate.GetType());

            bool result = chain.Build(certificate as X509Certificate2 ?? new X509Certificate2(certificate));

            _logger.Information("Build certificate chain result: {buildCertificateChainResult}", result);

            return result;
        }

        private async Task OnMessageReceived(object sender, BasicDeliverEventArgs @event)
        {
            _logger.Information("Received RabbitMQ message [RoutingKey: {routingKey} DeliveryTag: {deliveryTag}]", @event.RoutingKey, @event.DeliveryTag);
            try
            {
                await ProcessMessage(@event).ConfigureAwait(false);
                await _channel!.BasicAckAsync(@event.DeliveryTag, multiple: false).ConfigureAwait(false);
                _logger.Information("Processed and acknowledged RabbitMQ message [RoutingKey: {routingKey} DeliveryTag: {deliveryTag}]", @event.RoutingKey, @event.DeliveryTag);
            }
            catch (Exception exception)
            {
                bool isRetriable = false;

                bool requeue = isRetriable;
                await _channel!.BasicRejectAsync(@event.DeliveryTag, requeue).ConfigureAwait(false);
                _logger.Error(exception, "Could not process RabbitMQ message [RoutingKey: {routingKey} DeliveryTag: {deliveryTag}]", @event.RoutingKey, @event.DeliveryTag);
                _logger.Warning("Rejected RabbitMQ message [RoutingKey: {routingKey} DeliveryTag: {deliveryTag} Requeue: {requeue}]", @event.RoutingKey, @event.DeliveryTag, requeue);
                throw;
            }
        }

        protected virtual async Task ProcessMessage(BasicDeliverEventArgs @event)
        {
            using var scope = _scopeFactory.CreateScope();
            var consumer = scope.ServiceProvider.GetRequiredService<IEventBusConsumer<T>>();

            var body = @event.Body.ToArray();
            var eventMessage = Encoding.UTF8.GetString(body);
            _logger.Information("RabbitMQ [Message]: {message}", eventMessage);

            try
            {
                T context = JObject.Parse(eventMessage).ToObject<T>();
                if (context != null)
                {
                    await consumer.ConsumeAsync(@event, context).ConfigureAwait(false);
                    await Task.Delay(1500);
                }
                else
                {
                    _logger.Error("Deserialized context is null for message [RoutingKey: {routingKey} DeliveryTag: {deliveryTag}]", @event.RoutingKey, @event.DeliveryTag);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error processing message [RoutingKey: {routingKey} DeliveryTag: {deliveryTag}]", @event.RoutingKey, @event.DeliveryTag);
            }
        }
    }
}
