using SFA.DAS.FundingProjection.Api.Models.Requests;

namespace FundingProjection.Api.Testing.DataCreation;

public static class ProjectionRequest
{
    public static PostEmployerFundingProjectionRequest Create(int months = 6, decimal openingBalance = 1000, decimal? monthlyLevyIn = null)
    {
        return new PostEmployerFundingProjectionRequest
        {
            Months = months,
            CurrentLevyFunds = 1000,
            HistoricLevyIn = HistoricLevy.Generate(fixedMonthlyAmount: monthlyLevyIn)
        };
    }
}