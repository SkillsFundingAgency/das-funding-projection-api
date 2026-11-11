using SFA.DAS.FundingProjection.Api.Models;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Api.Projection;

public static class PaymentScheduleGenerator
{
    public static PaymentSchedule CreateFrom(DateOnly startPeriod, ApprenticeshipPaymentSummaryEntity summary)
    {
        return CreateForwardPaymentsForApprenticeship(summary, startPeriod);
    }

    private static PaymentSchedule CreateForwardPaymentsForApprenticeship(ApprenticeshipPaymentSummaryEntity summary, DateOnly now)
    {
        // TotalPaid includes the completion payment once it has been made, so split what is still owed
        // between the 20% completion payment and the remaining monthly instalments
        var completionPayment = summary.TotalCost * 0.2m;
        var remainingCost = Math.Max(0m, summary.TotalCost - summary.TotalPaid);
        var remainingCompletionPayment = Math.Min(completionPayment, remainingCost);
        var remainingMonthlyCost = remainingCost - remainingCompletionPayment;
        
        var firstPaymentPeriod = summary.StartDate.AddMonths(1).ToPeriod();
        var lastPaymentPeriod = summary.LastPaymentPeriod;

        var currentPaymentPeriod = now;
        
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
        
        // The month the last payment was recorded for is already handled, so only the months after it share what is left.
        // It can be later than the current month when a period end has run ahead of the calendar, and the months before it
        // are already in TotalPaid. A payment outside the schedule (early, or a late completion) has no month to fill
        var handledIndex = previousPaymentPeriod is { } paidPeriod
            ? remainingPaymentPeriods.FindIndex(p => p.PaymentPeriod == paidPeriod)
            : -1;

        if (handledIndex >= 0)
        {
            var handledPayment = remainingPaymentPeriods[handledIndex];

            if (handledIndex == remainingPaymentPeriods.Count - 1)
            {
                // the last payment includes the final payment, unless it fell short of it (e.g. a missed payment recorded as zero)
                var finalPayment = Math.Min(completionPayment, summary.LastPaymentAmount!.Value);
                handledPayment.ScheduledAmount = summary.LastPaymentAmount!.Value - finalPayment;
                handledPayment.FinalPayment = finalPayment;
                return paymentSchedule;
            }

            handledPayment.ScheduledAmount = summary.LastPaymentAmount!.Value;
            remainingPaymentPeriods = [.. remainingPaymentPeriods.Skip(handledIndex + 1)];
        }

        // avoid dividing by zero
        if (remainingPaymentPeriods is { Count: > 0 })
        {
            var monthlyPayment = remainingMonthlyCost / remainingPaymentPeriods.Count;
            foreach (var payment in remainingPaymentPeriods)
            {
                payment.ScheduledAmount = monthlyPayment;
            }
        }
        
        if (paymentSchedule.ScheduledPayments is { Count: > 0 })
        {
            // set the final payment
            paymentSchedule.ScheduledPayments[^1].FinalPayment = remainingCompletionPayment;
        }

        return paymentSchedule;
    }
}