using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Api.Projection;

public sealed record CoInvestmentEstimate(decimal LevyFunded, decimal EmployerCoInvestment, decimal DfeCoInvestment);

public interface ICoInvestmentEstimator
{
    Func<DateOnly, decimal, CoInvestmentEstimate> Prepare(List<ApprenticeshipPaymentSummaryEntity> apprenticeshipPaymentSummaries, List<PaymentSchedule> paymentSchedules);
}

public class CoInvestmentEstimator(IApprenticeshipOrderingStrategy apprenticeshipOrderingStrategy): ICoInvestmentEstimator
{
    public Func<DateOnly, decimal, CoInvestmentEstimate> Prepare(List<ApprenticeshipPaymentSummaryEntity> apprenticeshipPaymentSummaries, List<PaymentSchedule> paymentSchedules)
    {
        var payments = paymentSchedules
            .SelectMany(x => x.ScheduledPayments.Select(p => (Key: (x.ApprenticeshipId, p.PaymentPeriod), Payment: p)))
            .ToDictionary(x => x.Key, x => x.Payment);

        var orderedApprenticeships = apprenticeshipOrderingStrategy.Execute(apprenticeshipPaymentSummaries).ToList();

        return (period, availableLevy) => Estimate(period, availableLevy, orderedApprenticeships, payments);
    }

    private static CoInvestmentEstimate Estimate(
        DateOnly period,
        decimal availableLevy,
        List<ApprenticeshipPaymentSummaryEntity> orderedApprenticeships,
        Dictionary<(long, DateOnly), ScheduledPayment> payments)
    {
        var balance = Math.Max(availableLevy, 0m);
        var levyFunded = 0m;
        var employerCoInvestment = 0m;
        var dfeCoInvestment = 0m;

        foreach (var apprenticeship in orderedApprenticeships)
        {
            if (!payments.TryGetValue((apprenticeship.ApprenticeshipId, period), out var payment))
            {
                continue;
            }

            var amount = payment.ScheduledAmount + payment.FinalPayment;
            var fromLevy = Math.Min(balance, amount);
            balance -= fromLevy;
            levyFunded += fromLevy;

            var shortfall = amount - fromLevy;
            var employerShare = Rules
                .First(x => x.CanRun(apprenticeship))
                .CalculateCopayment(shortfall);
            employerCoInvestment += employerShare;
            dfeCoInvestment += shortfall - employerShare;
        }

        return new CoInvestmentEstimate(levyFunded, employerCoInvestment, dfeCoInvestment);
    }

    private static readonly List<CopaymentRule> Rules =
    [
        CopaymentRules.Percent25,
        CopaymentRules.Percent10,
        CopaymentRules.Percent5,
        CopaymentRules.DefaultPercent0,
    ];
}
