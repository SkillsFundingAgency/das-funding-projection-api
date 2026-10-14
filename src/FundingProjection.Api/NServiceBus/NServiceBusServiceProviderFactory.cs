using System.Diagnostics.CodeAnalysis;
using NServiceBus.ObjectBuilder.MSDependencyInjection;

namespace SFA.DAS.FundingProjection.Api.NServiceBus;

[ExcludeFromCodeCoverage]
public class NServiceBusServiceProviderFactory : IServiceProviderFactory<UpdateableServiceProvider>
{
    public UpdateableServiceProvider CreateBuilder(IServiceCollection services)
    {
        return new UpdateableServiceProvider(services);
    }

    public IServiceProvider CreateServiceProvider(UpdateableServiceProvider containerBuilder)
    {
        return containerBuilder;
    }
}