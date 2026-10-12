using SFA.DAS.FundingProjection.Api.Projection;

namespace SFA.DAS.FundingProjection.Api.Models.Requests;

public class PostEmployerFundingProjectionRequest
{
    public int Months { get; set; } = 6;
    public List<LevyInMonthSummary> HistoricLevyIn { get; set; } = [];
}