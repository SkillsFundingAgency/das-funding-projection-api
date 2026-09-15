namespace SFA.DAS.FundingProjection.Api.Core;

internal struct RouteNames
{
    public const string EmployerFundingProjection = $"{RouteElements.Api}/{RouteElements.Employer}";
}

internal struct RouteElements
{
    public const string Api = "api";
    public const string Employer = "employer";
    public const string FundingProjection = "funding-projection";
}