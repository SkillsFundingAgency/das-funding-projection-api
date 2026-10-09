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
                var levyIn = levyInProjections.FirstOrDefault(l => l.Period == currentPeriod)?.Amount ?? 0m;
                var committedLearnerCost = commitment?.TotalMonthlyPayments ?? 0m;
                var committedLearnerFinalPaymentCost = commitment?.TotalFinalPayments ?? 0m;
                var committedTransferOut = 0m;
                
                cumulativeBalance += levyIn;
                cumulativeBalance -= committedLearnerCost;
                cumulativeBalance -= committedLearnerFinalPaymentCost;
                cumulativeBalance -= committedTransferOut;
                
                var estimate = new MonthlyFundingBreakdown
                {
                    EmployerAccountId = accountId,
                    Month = currentPeriod.Month,
                    Year = currentPeriod.Year,
                    CommittedLearnerCost = committedLearnerCost,
                    CommittedLearnerFinalPaymentCost = committedLearnerFinalPaymentCost,
                    CommittedTransferOut = committedTransferOut,
                    LevyIn = levyIn,
                    ClosingBalance = cumulativeBalance,
                };
                
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