using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using SFA.DAS.FundingProjection.Api;
using SFA.DAS.FundingProjection.Domain.Services;

namespace FundingProjection.Api.Testing;

public class SqlWebApplication : WebApplicationFactory<Program>
{
    public Mock<IEventService> EventService { get; } = new ();
    
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configBuilder =>
            configBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Integration.json")
                .AddUserSecrets(Assembly.GetExecutingAssembly())
        );

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddTransient<IEventService>(_ => EventService.Object);
        });
    }
}