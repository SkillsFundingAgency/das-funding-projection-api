using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.FundingProjection.Data;

namespace FundingProjection.Api.Testing;

[SetUpFixture, Category("Integration")]
public class InMemoryOneTimeSetup
{
    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        await using var testServer = new SqlWebApplication();
        using var scope = testServer.Services.CreateScope();
        await using var dataContext = scope.ServiceProvider.GetRequiredService<FundingProjectionDataContext>();
        var configuration = testServer.Server.Services.GetService(typeof(IConfiguration)) as IConfiguration;
        var manager = new SqlTestDataManager(dataContext, null!, configuration.GetDbSchemaName());

        // don't do this for MSSQL as we'd have the pipeline create the db using the DAC - just use manager.WipeDataAsync() instead
        await manager.EnsureDeletedAsync();
        await manager.EnsureCreatedAsync();
    }
}