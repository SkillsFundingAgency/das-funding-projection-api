namespace SFA.DAS.FundingProjection.Api.Models.Responses;

public sealed record GetEmployerFundingProjectionByAccountIdResponse
{
    public List<MonthlyFundingBreakdown> FundingBreakdowns { get; init; } = [];
}