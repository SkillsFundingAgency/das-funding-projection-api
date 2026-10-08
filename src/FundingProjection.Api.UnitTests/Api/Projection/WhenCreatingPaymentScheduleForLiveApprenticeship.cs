using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Projection;

public class WhenCreatingPaymentScheduleForLiveApprenticeship
{
    [Test, MoqAutoData]
    public void Then_An_Apprenticeship_That_Has_Just_Started_Produces_Scheduled_Payments_In_The_Future()
    {
        // arrange
        var now = DateTime.UtcNow;
        var summary = new ApprenticeshipPaymentSummaryEntity
        {
            ApprenticeshipId = 1,
            AccountId = 1,
            TotalCost = 375m,
            TotalPaid = 0m,
            StartDate = now.AddMonths(0),
            EndDate = now.AddMonths(2),
            LastPaymentDate = null,
            LastPaymentAmount = null,
            Status = ApprenticeshipStatus.Live,
        };

        // act
        var result = PaymentScheduleGenerator.CreateFrom(summary);

        // assert
        result.ApprenticeshipId.Should().Be(1);
        result.ScheduledPayments.Should().HaveCount(3);

        result.ScheduledPayments.Should().AllSatisfy(x => x.ScheduledAmount.Should().Be(100m));
        result.ScheduledPayments.Take(2).Should().AllSatisfy(x => x.FinalPayment.Should().Be(0m));
        result.ScheduledPayments[^1].FinalPayment.Should().Be(75m);

        for (var i = 0; i < 3; i++)
        {
            result.ScheduledPayments[i].SourcePeriod.Should().Be(now.AddMonths(i).ToPeriod());
            result.ScheduledPayments[i].PaymentPeriod.Should().Be(now.AddMonths(i+1).ToPeriod());    
        }
    }
    
    [Test, MoqAutoData]
    public void Then_An_Apprenticeship_That_Started_A_Month_Ago_Has_Current_And_Future_Scheduled_Payments()
    {
        // arrange
        var now = DateTime.UtcNow;
        var summary = new ApprenticeshipPaymentSummaryEntity
        {
            ApprenticeshipId = 1,
            AccountId = 1,
            TotalCost = 375m,
            TotalPaid = 0m,
            StartDate = now.AddMonths(-1),
            EndDate = now.AddMonths(1),
            LastPaymentDate = null,
            LastPaymentAmount = null,
            Status = ApprenticeshipStatus.Live,
        };

        // act
        var result = PaymentScheduleGenerator.CreateFrom(summary);

        // assert
        result.ApprenticeshipId.Should().Be(1);
        result.ScheduledPayments.Should().HaveCount(3);

        result.ScheduledPayments.Should().AllSatisfy(x => x.ScheduledAmount.Should().Be(100m));
        result.ScheduledPayments.Take(2).Should().AllSatisfy(x => x.FinalPayment.Should().Be(0m));
        result.ScheduledPayments[^1].FinalPayment.Should().Be(75m);

        for (var i = 0; i < 3; i++)
        {
            result.ScheduledPayments[i].SourcePeriod.Should().Be(now.AddMonths(i-1).ToPeriod());
            result.ScheduledPayments[i].PaymentPeriod.Should().Be(now.AddMonths(i).ToPeriod());    
        }
    }
    
    [Test, MoqAutoData]
    public void Then_An_Apprenticeship_That_Ends_In_The_Current_Period_Produces_Only_The_Remaining_Payments()
    {
        // arrange
        var now = DateTime.UtcNow;
        var summary = new ApprenticeshipPaymentSummaryEntity
        {
            ApprenticeshipId = 1,
            AccountId = 1,
            TotalCost = 375m,
            TotalPaid = 100m,
            StartDate = now.AddMonths(-2),
            EndDate = now.AddMonths(0),
            LastPaymentDate = now.AddMonths(-1),
            LastPaymentAmount = 100m,
            Status = ApprenticeshipStatus.Live,
        };

        // act
        var result = PaymentScheduleGenerator.CreateFrom(summary);

        // assert
        result.ApprenticeshipId.Should().Be(1);
        result.ScheduledPayments.Should().HaveCount(2);

        result.ScheduledPayments[0].ScheduledAmount.Should().Be(100m);
        result.ScheduledPayments[0].FinalPayment.Should().Be(0m);
        result.ScheduledPayments[0].SourcePeriod.Should().Be(now.AddMonths(-1).ToPeriod());
        result.ScheduledPayments[0].PaymentPeriod.Should().Be(now.ToPeriod());
        
        result.ScheduledPayments[1].ScheduledAmount.Should().Be(100m);
        result.ScheduledPayments[1].FinalPayment.Should().Be(75m);
        result.ScheduledPayments[1].SourcePeriod.Should().Be(now.ToPeriod());
        result.ScheduledPayments[1].PaymentPeriod.Should().Be(now.AddMonths(1).ToPeriod());
    }
}