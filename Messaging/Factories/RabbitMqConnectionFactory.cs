using Messaging.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.Factories
{
    public class RabbitMqConnectionFactory
    {
        private readonly IOptions<RabbitMQOptions> _options;
        private readonly ILogger _logger;

        public RabbitMqConnectionFactory(IOptions<RabbitMQOptions> options, 
            ILogger logger)
        {
            _options = options;
            _logger = logger;
        }

        public async Task<IConnection> CreateConnectionAsync()
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
    }

}
