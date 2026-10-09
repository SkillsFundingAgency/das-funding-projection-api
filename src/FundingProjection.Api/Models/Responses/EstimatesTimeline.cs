namespace SFA.DAS.FundingProjection.Api.Models.Responses;

public class EstimatesTimeline
{
    public long AccountId { get; set; }
    public List<MonthlyFundingBreakdown> Projections { get; set; } = [];
}