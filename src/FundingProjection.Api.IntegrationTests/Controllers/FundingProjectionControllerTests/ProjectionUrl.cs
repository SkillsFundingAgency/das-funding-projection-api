namespace FundingProjection.Api.IntegrationTests.Controllers.FundingProjectionControllerTests;

// TODO: should be replaced with contracts
public static class ProjectionUrl
{
    public static Uri Create(long accountId)
    {
        return new Uri($"api/employer/{accountId}/funding-projection", UriKind.Relative);
    }
}