namespace SFA.DAS.FundingProjection.Api.Projection;

public static class CommitmentsProjector
{
    public static List<CommittedLevyProjection> CreateProjection(DateOnly now, int months, List<PaymentSchedule> scheduledPayments)
        => Project(scheduledPayments.SelectMany(x => x.ScheduledPayments), now.ToPeriod(), months);

    private static List<CommittedLevyProjection> Project(
        IEnumerable<ScheduledPayment> scheduledPayments, DateOnly startPeriod, int months)
    {
        var projections = new CommittedLevyProjection[months];
        for (var i = 0; i < months; i++)
        {
            projections[i] = new CommittedLevyProjection
            {
                Period = startPeriod.AddMonths(i)
            };
        }

        foreach (var scheduledPayment in scheduledPayments)
        {
            var offset = (scheduledPayment.PaymentPeriod.Year - startPeriod.Year) * 12
                         + (scheduledPayment.PaymentPeriod.Month - startPeriod.Month);

            if ((uint) offset >= (uint) months) continue;

            projections[offset].TotalMonthlyPayments += scheduledPayment.ScheduledAmount;
            projections[offset].TotalFinalPayments += scheduledPayment.FinalPayment;
        }

        return [.. projections];
    }
}