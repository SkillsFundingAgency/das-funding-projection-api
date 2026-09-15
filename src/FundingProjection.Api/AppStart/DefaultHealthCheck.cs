using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SFA.DAS.FundingProjection.Data;

namespace SFA.DAS.FundingProjection.Api.AppStart;

[ExcludeFromCodeCoverage]
public class DefaultHealthCheck(IFundingProjectionDataContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.ProviderName?.EndsWith("InMemory", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            return HealthCheckResult.Healthy();
        }

        try
        {
            await dbContext.Ping(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch
        {
            return HealthCheckResult.Unhealthy();
        }
    }
}