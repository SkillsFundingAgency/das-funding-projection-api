using SFA.DAS.EmployerFinance.Types.Models;
using SFA.DAS.FundingProjection.Api.Models.Requests;
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
            List<LevyInMonthSummary> levyInProjections,
            IExpiredFunds expiredFunds,
            List<CalendarPeriodAmount> levyFundsInForExpiry,
            List<CalendarPeriodAmount> paymentFundsOutForExpiry,
            List<CalendarPeriodAmount> shortTermExpiredFunds,
            List<CalendarPeriodAmount> longTermExpiredFunds,
            bool currentMonthLevyInReceived
            )
        {
            var closingBalance = openingBalance;
            var currentPeriod = startPeriod;
            var estimates = new List<MonthlyFundingBreakdown>();

            var fundsIn = levyFundsInForExpiry.ToDictionary(c => new CalendarPeriod(c.Year, c.Month), c => c.Amount);
            var fundsOut = paymentFundsOutForExpiry.ToDictionary(c => new CalendarPeriod(c.Year, c.Month), c => c.Amount);
            var expired = shortTermExpiredFunds.Concat(longTermExpiredFunds)
                .GroupBy(c => new CalendarPeriod(c.Year, c.Month))
                .ToDictionary(g => g.Key, g => g.Sum(c => c.Amount));
            
            
            for (var i = 0; i < months; i++)
            {
                var commitment = commitmentProjections.FirstOrDefault(c => c.Period == currentPeriod);
                var transferOut = transferOutProjections.FirstOrDefault(t => t.Period == currentPeriod);
                var levyInSummary = new LevyInSummary
                {
                    LevyIn = levyInProjections.FirstOrDefault(l => l.Period == currentPeriod)?.Amount ?? 0m
                };

                var totalFundsOut = (commitment?.TotalFinalPayments ?? 0m) + (commitment?.TotalMonthlyPayments ?? 0m) +
                                    (transferOut?.TotalFinalPayments ?? 0m) + (transferOut?.TotalMonthlyPayments ?? 0m);

                var calendarPeriod = new CalendarPeriod(currentPeriod.Year, currentPeriod.Month);
                fundsIn.TryAdd(calendarPeriod, levyInSummary.LevyIn);
                fundsOut.TryAdd(calendarPeriod, totalFundsOut);

                // GetExpiringFunds mutates the dictionaries it is given, so pass copies to keep the running totals intact
                var expiryForPeriod = expiredFunds.GetExpiringFunds(
                    new Dictionary<CalendarPeriod, decimal>(fundsIn),
                    new Dictionary<CalendarPeriod, decimal>(fundsOut),
                    expired,
                    12);

                var expiredAmount = expiryForPeriod.TryGetValue(calendarPeriod, out var amount) ? amount : 0m;

                // record this month's expiry so it is treated as fixed when later months are calculated
                expired.TryAdd(calendarPeriod, expiredAmount);

                var levyOutSummary = new LevyOutSummary
                {
                    CommittedLearnerCost = commitment?.TotalMonthlyPayments ?? 0m,
                    CommittedLearnerFinalPaymentCost = commitment?.TotalFinalPayments ?? 0m,
                    CommittedTransferOut = transferOut?.TotalMonthlyPayments ?? 0m,
                    CommittedTransferOutFinalPaymentCost = transferOut?.TotalFinalPayments ?? 0m,
                    ExpiredLevy = expired[calendarPeriod],
                };
                
                // once this month's levy has come in it is already in the opening balance, so it is shown but not added again
                var levyInToAdd = i == 0 && currentMonthLevyInReceived ? 0m : levyInSummary.Total;
                closingBalance = closingBalance + levyInToAdd - levyOutSummary.Total;
                
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