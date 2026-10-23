namespace SFA.DAS.FundingProjection.Api.Core;

public struct RouteNames
{
    public const string EmployerFundingProjection = $"{RouteElements.Api}/{RouteElements.Employer}";
    public const string ApprenticeshipPaymentSummary = $"{RouteElements.Api}/{RouteElements.Employer}";
    public const string Jobs = $"{RouteElements.Api}/{RouteElements.Jobs}";
}

public struct RouteElements
{
    public const string Api = "api";
    public const string Employer = "employer";
    public const string FundingProjection = "funding-projection";
    public const string ApprenticeshipPaymentSummary = "apprenticeship-payment-summary";
    public const string Jobs = "jobs";
}