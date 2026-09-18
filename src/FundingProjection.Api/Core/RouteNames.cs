namespace SFA.DAS.FundingProjection.Api.Core;

internal struct RouteNames
{
    public const string EmployerFundingProjection = $"{RouteElements.Api}/{RouteElements.Employer}";
    public const string Learners = $"{RouteElements.Api}/{RouteElements.Employer}";
    public const string Jobs = $"{RouteElements.Api}/{RouteElements.Jobs}";
}

internal struct RouteElements
{
    public const string Api = "api";
    public const string Employer = "employer";
    public const string FundingProjection = "funding-projection";
    public const string Learners = "learners";
    public const string Jobs = "jobs";
}