namespace SFA.DAS.FundingProjection.Api.Models.Responses;

public class EstimatesTimeline
{
    public long AccountId { get; set; }
    public decimal TotalEstimatedCommitedCost { get; set; }
    public decimal TotalEstimatedCommitedTransferCost { get; set; }
    public List<MonthlyFundingBreakdown> Projections { get; set; } = [];
}