using System.Security.Authentication;

namespace Messaging.Configuration
{
    public class RabbitMQSSLOptions
    {
        public bool Enabled { get; set; }
        public SslProtocols? Protocols { get; set; }
        public RabbitMQCertificateOptions? KeyStore { get; set; }
        public RabbitMQCertificateOptions? TrustStore { get; set; }
    }
}
