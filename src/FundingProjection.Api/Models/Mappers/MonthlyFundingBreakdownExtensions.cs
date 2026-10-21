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
            decimal totalRemainingCost,
            decimal totalRemainingTransferCost,
            List<CommittedLevyProjection> commitmentProjections,
            List<CommittedLevyProjection> transferOutProjections,
            List<LevyInMonthSummary> levyInProjections
            )
        {
            var closingBalance = openingBalance;
            var currentPeriod = startPeriod;
            var estimates = new List<MonthlyFundingBreakdown>();
            for (var i = 0; i < months; i++)
            {
                var commitment = commitmentProjections.FirstOrDefault(c => c.Period == currentPeriod);
                var transferOut = transferOutProjections.FirstOrDefault(t => t.Period == currentPeriod);
                var levyInSummary = new LevyInSummary
                {
                    LevyIn = levyInProjections.FirstOrDefault(l => l.Period == currentPeriod)?.Amount ?? 0m
                };

                var levyOutSummary = new LevyOutSummary
                {
                    CommittedLearnerCost = commitment?.TotalMonthlyPayments ?? 0m,
                    CommittedLearnerFinalPaymentCost = commitment?.TotalFinalPayments ?? 0m,
                    CommittedTransferOut = transferOut?.TotalMonthlyPayments ?? 0m,
                    CommittedTransferOutFinalPaymentCost = transferOut?.TotalFinalPayments ?? 0m,
                    ExpiredLevy = 0m,
                };
                
                closingBalance = closingBalance + levyInSummary.Total - levyOutSummary.Total;
                var estimate = new MonthlyFundingBreakdown
                {
                    EmployerAccountId = accountId,
                    Month = currentPeriod.Month,
                    Year = currentPeriod.Year,
                    LevyInSummary = levyInSummary,
                    LevyOutSummary = levyOutSummary,
                    ClosingBalance = closingBalance,
                };
                
                currentPeriod = currentPeriod.AddMonths(1);
                estimates.Add(estimate);
            }

            return new EstimatesTimeline
            {
                AccountId = accountId,
                TotalEstimatedCommitedCost = totalRemainingCost,
                TotalEstimatedCommitedTransferCost = totalRemainingTransferCost,
                Projections = estimates
            };
        }
    }
}