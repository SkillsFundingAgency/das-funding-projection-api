using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.Projection;

public static class PaymentScheduleGenerator
{
    public static PaymentSchedule CreateFrom(ApprenticeshipPaymentSummaryEntity summary)
    {
        var now = DateTime.UtcNow;
        return summary.Status switch
        {
            ApprenticeshipStatus.Completed => CreateForCompletedApprenticeship(summary, now),
            ApprenticeshipStatus.Live => CreateForLiveApprenticeship(summary, now),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static PaymentSchedule CreateForCompletedApprenticeship(ApprenticeshipPaymentSummaryEntity summary, DateTime now) => CreateForLiveApprenticeship(summary, now);

    private static PaymentSchedule CreateForLiveApprenticeship(ApprenticeshipPaymentSummaryEntity summary, DateTime now)
    {
        var remainingCost = summary.TotalCost * 0.8m - summary.TotalPaid;
        
        var firstPaymentPeriod = summary.StartDate.AddMonths(1).ToPeriod();
        var lastPaymentPeriod = summary.EndDate.AddMonths(1).ToPeriod();

        var thisPaymentPeriod = now.ToPeriod();
        var currentPaymentPeriod = thisPaymentPeriod;
        
        var paymentSchedule = new PaymentSchedule { ApprenticeshipId = summary.ApprenticeshipId };
        
        // from current month, just create the required number of remaining payments (no amounts)
        while (currentPaymentPeriod <= lastPaymentPeriod)
        {
            if (currentPaymentPeriod < firstPaymentPeriod)
            {
                // we're not at the start of the apprenticeship
                currentPaymentPeriod = currentPaymentPeriod.AddMonths(1);
                continue;
            };
            
            paymentSchedule.ScheduledPayments.Add(new ScheduledPayment
            {
                PaymentPeriod = currentPaymentPeriod,
                SourcePeriod = currentPaymentPeriod.AddMonths(-1),
                ScheduledAmount = 0m,
                FinalPayment = 0m
            });
                                
            currentPaymentPeriod = currentPaymentPeriod.AddMonths(1);
        }
        
        var previousPaymentPeriod = summary.LastPaymentDate?.ToPeriod();
        var remainingPaymentPeriods = paymentSchedule.ScheduledPayments;
        
        // current month already handled, reduce the number of remaining payments to calc
        if (thisPaymentPeriod == previousPaymentPeriod)
        {
            if (remainingPaymentPeriods.Count == 1)
            {
                // the last payment includes the final payment
                paymentSchedule.ScheduledPayments[0].ScheduledAmount = summary.LastPaymentAmount!.Value - (summary.TotalCost * 0.2m);
            }
            else
            {
                paymentSchedule.ScheduledPayments[0].ScheduledAmount = summary.LastPaymentAmount!.Value;
            }
            remainingPaymentPeriods = [.. remainingPaymentPeriods.Skip(1)];
        }

        // avoid dividing by zero
        if (remainingPaymentPeriods is { Count: > 0 })
        {
            var monthlyPayment = remainingCost / remainingPaymentPeriods.Count;
            foreach (var payment in remainingPaymentPeriods)
            {
                payment.ScheduledAmount = monthlyPayment;
            }
        }
        
        if (paymentSchedule.ScheduledPayments is { Count: > 0 })
        {
            // set the final payment
            paymentSchedule.ScheduledPayments[^1].FinalPayment = summary.TotalCost * 0.2m;
        }

        return paymentSchedule;
    }
}