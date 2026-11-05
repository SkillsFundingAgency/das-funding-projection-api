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
        var lastPaymentPeriod = summary.EndDate.AddMonths(1).ToPeriod();

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
        
        // current month already handled, reduce the number of remaining payments to calc
        if (now == previousPaymentPeriod)
        {
            if (remainingPaymentPeriods.Count == 1)
            {
                // the last payment includes the final payment
                paymentSchedule.ScheduledPayments[0].ScheduledAmount = summary.LastPaymentAmount!.Value - completionPayment;
                paymentSchedule.ScheduledPayments[0].FinalPayment = completionPayment;
                return paymentSchedule;
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