using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReportingService.Clients.Impl;
using ReportingService.Clients;
using ReportingService.Tests.Fakes;
using ReportingService.Tests;

namespace TransactionsService.Tests.Integration.Postgres
{
    public class WebAppFactory : WebApplicationFactory<Program>
    {

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
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
            });
        }


    }
}
