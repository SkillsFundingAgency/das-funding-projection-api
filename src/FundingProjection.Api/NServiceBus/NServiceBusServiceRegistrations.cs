using System.Diagnostics.CodeAnalysis;
using NServiceBus;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using Endpoint = NServiceBus.Endpoint;

namespace SFA.DAS.FundingProjection.Api.NServiceBus;

[ExcludeFromCodeCoverage]
public static class NServiceBusServiceRegistrations
{
    private const string EndpointName = "SFA.DAS.EmployerFinance";
    
    public static void StartNServiceBus(this UpdateableServiceProvider services, IConfiguration configuration)
    {
        var endpointConfiguration = new EndpointConfiguration(EndpointName)
            .UseErrorQueue($"{EndpointName}-errors")
            .UseInstallers()
            .UseMessageConventions()
            .UseServicesBuilder(services)
            .UseNewtonsoftJsonSerializer()
            .UseLicense(configuration["NServiceBusLicense"])
            .UseConnectionString(configuration["ServiceBusConnectionString"]);
        
        var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
        services
            .AddSingleton(endpoint)
            .AddSingleton<IMessageSession>(p => p.GetService<IEndpointInstance>()!)
            .AddHostedService<NServiceBusHostedService>();
    }
}