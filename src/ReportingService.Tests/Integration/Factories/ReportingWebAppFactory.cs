using FluentAssertions.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ReportingService.Clients;
using ReportingService.Clients.Impl;
using ReportingService.Tests.Fakes;
using Shared.Auth;
using Shared.Auth.Impl;
using Shared.Testing.Factories;
using Shared.Testing.Utils;

namespace ReportingService.Tests.Integration.Factories
{
    public class ReportingWebAppFactory : CustomWebApplicationFactory<Program>
    {
        public ReportingWebAppFactory()
            : base(services =>
            {
                services.RemoveAll<ITransactionsClient>();
                services.RemoveAll<IAuthTokenClient>();
                services.RemoveAll<ServiceAuthHandler>();

                services.AddTransient<FakeServiceAuthHandler>();

                services.AddHttpClient<ITransactionsClient, TransactionsClient>("TestClient", client =>
                {
                    client.BaseAddress = new Uri("http://localhost"); 
                })
                    .AddHttpMessageHandler<FakeServiceAuthHandler>()
                    .ConfigurePrimaryHttpMessageHandler(() => new FakeTransactionsHandler());

                services.AddTransient<TransactionsClient>(provider =>
                {
                    var factory = provider.GetRequiredService<IHttpClientFactory>();
                    var client = factory.CreateClient("TestClient");
                    return new TransactionsClient(client);
                });


                services.PostConfigureAll<AuthenticationOptions>(opts =>
                {
                    opts.DefaultAuthenticateScheme = "Test";
                    opts.DefaultChallengeScheme = "Test";
                });



                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestingAuthHandler>("Test", _ => { });
            })
        {
        }
    }

}
