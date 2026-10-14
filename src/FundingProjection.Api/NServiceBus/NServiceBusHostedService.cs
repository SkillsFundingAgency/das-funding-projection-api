using System.Diagnostics.CodeAnalysis;
using NServiceBus;

namespace SFA.DAS.FundingProjection.Api.NServiceBus;

[ExcludeFromCodeCoverage]
public class NServiceBusHostedService(IEndpointInstance endpoint) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => endpoint.Stop();
}