using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Api.Projection;

public interface ICoInvestmentEstimator
{
    void Estimate(EstimatesTimeline timeline, List<ApprenticeshipPaymentSummaryEntity> apprenticeshipPaymentSummaries, List<PaymentSchedule> scheduledPayments, decimal openingLevyBalance);
}

public class CoInvestmentEstimator(IApprenticeshipOrderingStrategy apprenticeshipOrderingStrategy): ICoInvestmentEstimator
{
    public void Estimate(EstimatesTimeline timeline, List<ApprenticeshipPaymentSummaryEntity> apprenticeshipPaymentSummaries, List<PaymentSchedule> scheduledPayments, decimal openingLevyBalance)
    {
        var paymentSchedules = scheduledPayments
            .Select(x => x.ScheduledPayments.Select(p => new KeyValuePair<string, ScheduledPayment>($"{x.ApprenticeshipId}-{p.PaymentPeriod}", p)))
            .SelectMany(x => x)
            .ToDictionary(x => x.Key, x => x.Value);

        var orderedApprenticeships = apprenticeshipOrderingStrategy.Execute(apprenticeshipPaymentSummaries).ToList(); 

        for (var index = 0; index < timeline.Projections.Count; index++)
        {
            var projectedPeriod = timeline.Projections[index];
            var balance = index == 0 
                ? openingLevyBalance
                : timeline.Projections[index-1].ClosingBalance;
            var currentPeriod = new DateOnly(projectedPeriod.Year, projectedPeriod.Month, 1);
            var copay = 0m;
            foreach (var apprenticeship in orderedApprenticeships)
            {
                if (!paymentSchedules.TryGetValue($"{apprenticeship.ApprenticeshipId}-{currentPeriod}", out var payment))
                {
                    continue;
                }
                
                balance -= payment.ScheduledAmount + payment.FinalPayment;
                if (balance >= 0)
                {
                    continue;
                }
                
                copay += CalculateCoPayment(apprenticeship, balance, payment);
            }
            projectedPeriod.CoInvestmentCosts = copay;
        }
    }

    private static decimal CalculateCoPayment(ApprenticeshipPaymentSummaryEntity apprenticeship, decimal balance, ScheduledPayment payment)
    {
        var amount = payment.ScheduledAmount + payment.FinalPayment;
        var value = Math.Min(amount + (0 - (balance + amount)), amount);
        return Rules
            .First(x => x.CanRun(apprenticeship))
            .CalculateCopayment(value);
    }

    private static readonly List<CopaymentRule> Rules =
    [
        CopaymentRules.Percent25,
        CopaymentRules.Percent10,
        CopaymentRules.Percent5,
        CopaymentRules.DefaultPercent0,
    ];
}