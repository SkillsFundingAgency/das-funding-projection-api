using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Projection;

public class WhenCreatingPaymentScheduleForCompletedApprenticeship
{
    [Test]
    public void Then_An_Apprenticeship_That_Ended_Last_Month_Produces_The_Final_Payment_In_The_Current_Month()
    {
        // arrange
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var summary = new ApprenticeshipPaymentSummaryEntity
        {
            ApprenticeshipId = 1,
            AccountId = 1,
            TotalCost = 375m,
            TotalPaid = 200m,
            StartDate = now.AddMonths(-3),
            EndDate = now.AddMonths(-1),
            LastPaymentDate = now.AddMonths(-1),
            LastPaymentAmount = 100m,
            Status = ApprenticeshipStatus.Completed,
        };

        // act
        var result = PaymentScheduleGenerator.CreateFrom(summary);

        // assert
        result.ApprenticeshipId.Should().Be(1);
        result.ScheduledPayments.Should().HaveCount(1);

        result.ScheduledPayments[0].ScheduledAmount.Should().Be(100m);
        result.ScheduledPayments[0].FinalPayment.Should().Be(75m);
        result.ScheduledPayments[0].SourcePeriod.Should().Be(now.AddMonths(-1).ToPeriod());
        result.ScheduledPayments[0].PaymentPeriod.Should().Be(now.ToPeriod());
    }
    
    [Test]
    public void Then_An_Apprenticeship_Which_Completed_Last_Month_Produces_A_Scheduled_Payment_Even_Though_It_Has_Already_Been_Paid()
    {
        /*
         Essentially, we want the same output whether we're looking at data at the beginning of the month (when no payments exist for that month)
         or at the end of the month (when payments have been made during that month).          
         */
        
        // arrange
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var summary = new ApprenticeshipPaymentSummaryEntity
        {
            ApprenticeshipId = 1,
            AccountId = 1,
            TotalCost = 375m,
            TotalPaid = 375m,
            StartDate = now.AddMonths(-3),
            EndDate = now.AddMonths(-1),
            LastPaymentDate = now,
            LastPaymentAmount = 175m,
            Status = ApprenticeshipStatus.Completed,
        };

        // act
        var result = PaymentScheduleGenerator.CreateFrom(summary);

        // assert
        result.ApprenticeshipId.Should().Be(1);
        result.ScheduledPayments.Should().HaveCount(1);

        result.ScheduledPayments[0].ScheduledAmount.Should().Be(100m);
        result.ScheduledPayments[0].FinalPayment.Should().Be(75m);
        result.ScheduledPayments[0].SourcePeriod.Should().Be(now.AddMonths(-1).ToPeriod());
        result.ScheduledPayments[0].PaymentPeriod.Should().Be(now.ToPeriod());
    }
    
    [Test]
    public void Then_An_Apprenticeship_Which_Completed_Two_Months_Ago_Produces_No_Scheduled_Payments()
    {
        // arrange
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var summary = new ApprenticeshipPaymentSummaryEntity
        {
            ApprenticeshipId = 1,
            AccountId = 1,
            TotalCost = 375m,
            TotalPaid = 375m,
            StartDate = now.AddMonths(-4),
            EndDate = now.AddMonths(-2),
            LastPaymentDate = now.AddMonths(-1),
            LastPaymentAmount = 175m,
            Status = ApprenticeshipStatus.Completed,
        };

        // act
        var result = PaymentScheduleGenerator.CreateFrom(summary);

        // assert
        result.ApprenticeshipId.Should().Be(1);
        result.ScheduledPayments.Should().HaveCount(0);
    }
}