using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Api.Projection;

namespace SFA.DAS.FundingProjection.Api.Models.Mappers;

public static class EstimatesTimelineExtensions
{
    extension(EstimatesTimeline)
    {
        public static EstimatesTimeline From(
            long accountId,
            DateOnly startPeriod,
            int months,
            decimal openingBalance,
            List<CommittedLevyProjection> commitmentProjections, List<LevyInMonthSummary> levyInProjections)
        {
            var cumulativeBalance = openingBalance;
            var currentPeriod = startPeriod;
            var estimates = new List<MonthlyFundingBreakdown>();
            for (var i = 0; i < months; i++)
            {
                var commitment = commitmentProjections.FirstOrDefault(c => c.Period == currentPeriod);
                var levyIn = levyInProjections.FirstOrDefault(l => l.Period == currentPeriod);
                var estimate = new MonthlyFundingBreakdown
                {
                    EmployerAccountId = accountId,
                    Month = currentPeriod.Month,
                    Year = currentPeriod.Year,
                    OpeningBalance = cumulativeBalance,
                    CommittedLearnerCost = commitment?.TotalMonthlyPayments ?? 0m,
                    CommittedLearnerFinalPaymentCost = commitment?.TotalFinalPayments ?? 0m,
                    CommittedTransferOut = 0m,
                    LevyIn = levyIn?.Amount ?? 0m
                };
                cumulativeBalance += estimate.LevyIn;
                cumulativeBalance -= estimate.CommittedLearnerCost;
                cumulativeBalance -= estimate.CommittedLearnerFinalPaymentCost;
                cumulativeBalance -= estimate.CommittedTransferOut;
                currentPeriod = currentPeriod.AddMonths(1);
                estimates.Add(estimate);
            }

            return new EstimatesTimeline
            {
                AccountId = accountId,
                Projections = estimates
            };
        }
    }
}