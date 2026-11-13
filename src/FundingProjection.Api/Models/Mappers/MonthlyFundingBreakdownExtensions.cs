using SFA.DAS.EmployerFinance.Types.Models;
using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Api.Projection;

namespace SFA.DAS.FundingProjection.Api.Models.Mappers;

public static class EstimatesTimelineExtensions
{
    // levy received before the policy change expires after 24 months (long term), and from it after 12 months (short term)
    private static readonly CalendarPeriod PolicyChangePeriod = new(2026, 8);
    private const int LongTermExpiryPeriod = 24;
    private const int ShortTermExpiryPeriod = 12;

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
            bool currentMonthLevyInReceived,
            CurrentPeriodPayments? currentPeriodPayments = null,
            Func<DateOnly, decimal, CoInvestmentEstimate>? estimateCoInvestment = null
            )
        {
            var closingBalance = openingBalance;
            var currentPeriod = startPeriod;
            var estimates = new List<MonthlyFundingBreakdown>();

            var fundsIn = levyFundsInForExpiry.ToDictionary(c => new CalendarPeriod(c.Year, c.Month), c => c.Amount);
            var fundsOut = paymentFundsOutForExpiry.ToDictionary(c => new CalendarPeriod(c.Year, c.Month), c => c.Amount);
            var longTermExpired = ToExpiryDictionary(longTermExpiredFunds);
            var shortTermExpired = ToExpiryDictionary(shortTermExpiredFunds);

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

                // once this month's payments have been made the actual amounts are shown in place of the estimates
                var actualPayments = i == 0 ? currentPeriodPayments : null;

                // payments are taken from the balance carried in before this month's levy arrives, levy only covers
                // them while there is a balance and the shortfall is co-invested, so the balance never goes below zero
                var coInvestment = actualPayments != null
                    ? new CoInvestmentEstimate(totalFundsOut, actualPayments.CoInvestment, actualPayments.DfeCoInvestment)
                    : estimateCoInvestment?.Invoke(currentPeriod, closingBalance)
                      ?? FullyDfeFunded(totalFundsOut, closingBalance);

                var calendarPeriod = new CalendarPeriod(currentPeriod.Year, currentPeriod.Month);
                fundsIn.TryAdd(calendarPeriod, levyInSummary.LevyIn);
                fundsOut.TryAdd(calendarPeriod, coInvestment.LevyFunded);

                // GetExpiringFunds mutates the dictionaries it is given, so pass copies to keep the running totals intact.
                // The funds out copy is shared so the long term run spends first and the short term run gets what is left
                var fundsOutForPeriod = new Dictionary<CalendarPeriod, decimal>(fundsOut);

                var longTermExpiry = expiredFunds.GetExpiringFunds(
                    fundsIn.Where(c => c.Key < PolicyChangePeriod).ToDictionary(c => c.Key, c => c.Value),
                    fundsOutForPeriod,
                    longTermExpired,
                    LongTermExpiryPeriod);

                var shortTermExpiry = expiredFunds.GetExpiringFunds(
                    fundsIn.Where(c => c.Key >= PolicyChangePeriod).ToDictionary(c => c.Key, c => c.Value),
                    fundsOutForPeriod,
                    shortTermExpired,
                    ShortTermExpiryPeriod);

                // record this month's expiry so it is treated as fixed when later months are calculated
                longTermExpired.TryAdd(calendarPeriod, ExpiredAmountFor(longTermExpiry, calendarPeriod));
                shortTermExpired.TryAdd(calendarPeriod, ExpiredAmountFor(shortTermExpiry, calendarPeriod));

                var levyOutSummary = new LevyOutSummary
                {
                    CommittedLearnerCost = actualPayments?.LearnerCost ?? commitment?.TotalMonthlyPayments ?? 0m,
                    CommittedLearnerFinalPaymentCost = actualPayments?.LearnerFinalPaymentCost ?? commitment?.TotalFinalPayments ?? 0m,
                    CommittedTransferOut = actualPayments?.TransferOut ?? transferOut?.TotalMonthlyPayments ?? 0m,
                    CommittedTransferOutFinalPaymentCost = actualPayments?.TransferOutFinalPaymentCost ?? transferOut?.TotalFinalPayments ?? 0m,
                    ExpiredLevy = longTermExpired[calendarPeriod] + shortTermExpired[calendarPeriod],
                };
                
                // once this month's levy has come in it is already in the opening balance, so it is shown but not added again
                var levyInToAdd = i == 0 && currentMonthLevyInReceived ? 0m : levyInSummary.Total;
                // the same applies to this month's payments, so only the expired levy is taken off
                var levyOutToSubtract = actualPayments != null
                    ? levyOutSummary.ExpiredLevy
                    : coInvestment.LevyFunded + levyOutSummary.ExpiredLevy;
                closingBalance = closingBalance + levyInToAdd - levyOutToSubtract;
                
                var estimate = new MonthlyFundingBreakdown
                {
                    EmployerAccountId = accountId,
                    Month = currentPeriod.Month,
                    Year = currentPeriod.Year,
                    LevyInSummary = levyInSummary,
                    LevyOutSummary = levyOutSummary,
                    ClosingBalance = closingBalance,
                    CoInvestmentCosts = coInvestment.EmployerCoInvestment + coInvestment.DfeCoInvestment != 0m
                        ? coInvestment.EmployerCoInvestment
                        : null,
                    DfeCoInvestmentCost = coInvestment.DfeCoInvestment,
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

    
    private static CoInvestmentEstimate FullyDfeFunded(decimal totalFundsOut, decimal availableLevy)
    {
        var levyFunded = Math.Min(Math.Max(availableLevy, 0m), totalFundsOut);
        return new CoInvestmentEstimate(levyFunded, 0m, totalFundsOut - levyFunded);
    }

    private static Dictionary<CalendarPeriod, decimal> ToExpiryDictionary(List<CalendarPeriodAmount> expiredFunds) =>
        expiredFunds
            .GroupBy(c => new CalendarPeriod(c.Year, c.Month))
            .ToDictionary(g => g.Key, g => g.Sum(c => c.Amount));

    // negative values are adjustments rather than levy expiring, so they are ignored
    private static decimal ExpiredAmountFor(IDictionary<CalendarPeriod, decimal> expiringFunds, CalendarPeriod period) =>
        expiringFunds.TryGetValue(period, out var amount) && amount >= 0 ? amount : 0m;
}