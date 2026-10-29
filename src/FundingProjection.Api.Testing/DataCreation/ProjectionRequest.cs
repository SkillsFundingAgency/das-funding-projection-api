using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Api.Projection;

namespace FundingProjection.Api.Testing.DataCreation;

public static class ProjectionRequest
{
    public static PostEmployerFundingProjectionRequest Create(int months = 6, decimal openingBalance = 1000, decimal? monthlyLevyIn = null, DateOnly? startPeriod = null)
    {
        return new PostEmployerFundingProjectionRequest
        {
            StartPeriod = startPeriod,
            Months = months,
            CurrentLevyFunds = openingBalance,
            HistoricLevyIn = HistoricLevy.Generate(fixedMonthlyAmount: monthlyLevyIn)
        };
    }
    
    public static PostEmployerFundingProjectionRequest Create(int months, decimal openingBalance, List<LevyInMonthSummary> historicLevyIn)
    {
        return new PostEmployerFundingProjectionRequest
        {
            Months = months,
            CurrentLevyFunds = openingBalance,
            HistoricLevyIn = historicLevyIn
        };
    }
}