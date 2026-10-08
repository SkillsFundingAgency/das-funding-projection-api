using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Api.Projection;

public class CommitmentsProjector
{
    public static List<CommittedLevyProjection> CreateProjection(List<ApprenticeshipPaymentSummaryEntity> apprenticeshipSummaries, int months)
    {
        var now = DateTime.UtcNow;
        var startPeriod = now.ToPeriod();
            
        var scheduledPayments = apprenticeshipSummaries
            .Select(PaymentScheduleGenerator.CreateFrom)
            .SelectMany(x => x.ScheduledPayments);

        var projections = Project(scheduledPayments, startPeriod, months);
        return [.. projections.OrderBy(x => x.Period)];
    }

    internal static List<CommittedLevyProjection> Project(IEnumerable<ScheduledPayment> scheduledPayments, DateOnly startPeriod, int months)
    {
        var lookup = new Dictionary<DateOnly, CommittedLevyProjection>();
        for (var i=0; i<months; i++)
        {
            var period = startPeriod;
            lookup.Add(period, new CommittedLevyProjection
            {
                Period = period
            });
            startPeriod = startPeriod.AddMonths(1);
        }

        foreach (var scheduledPayment in scheduledPayments)
        {
            if (lookup.TryGetValue(scheduledPayment.PaymentPeriod, out var projection))
            {
                projection.TotalMonthlyPayments += scheduledPayment.ScheduledAmount;
                projection.TotalFinalPayments += scheduledPayment.FinalPayment;
            }
        }

        return [.. lookup.Select(x => x.Value)];
    }
}