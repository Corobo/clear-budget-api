using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using ReportingService.Clients;
using ReportingService.Clients.Impl;
using ReportingService.Tests.Fakes;
using Shared.Testing.Factories;
using Shared.Testing.Utils;

namespace ReportingService.Tests.Integration.Factorie
{
    public class ReportingWebAppFactory : CustomWebApplicationFactory<Program>
    {
        public ReportingWebAppFactory()
            : base(services =>
            {
                services.PostConfigureAll<AuthenticationOptions>(opts =>
                {
                    opts.DefaultAuthenticateScheme = "Test";
                    opts.DefaultChallengeScheme = "Test";
                });

                services.AddHttpClient<ITransactionsClient, TransactionsClient>()
                        .ConfigurePrimaryHttpMessageHandler(() => new FakeTransactionsHandler());

                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestingAuthHandler>("Test", _ => { });
            })
        {
        }
    }
}
