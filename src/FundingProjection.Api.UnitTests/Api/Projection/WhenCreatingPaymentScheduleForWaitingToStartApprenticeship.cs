using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Projection;

public class WhenCreatingPaymentScheduleForWaitingToStartApprenticeship
{
    [Test, MoqAutoData]
    public void Then_An_Apprenticeship_That_Is_Waiting_To_Start_Produces_Scheduled_Payments_In_The_Future()
    {
        // arrange
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var summary = new ApprenticeshipPaymentSummaryEntity
        {
            ApprenticeshipId = 1,
            AccountId = 1,
            TotalCost = 375m,
            TotalPaid = 0m,
            StartDate = now.AddMonths(1),
            EndDate = now.AddMonths(3),
            LastPaymentDate = null,
            LastPaymentAmount = null,
            Status = ApprenticeshipStatus.WaitingToStart,
        };

        // act
        var result = PaymentScheduleGenerator.CreateFrom(summary);

        // assert
        result.ApprenticeshipId.Should().Be(1);
        result.ScheduledPayments.Should().HaveCount(3);
        
        result.ScheduledPayments[0].SourcePeriod.Should().Be(now.AddMonths(1).ToPeriod());
        result.ScheduledPayments[0].PaymentPeriod.Should().Be(now.AddMonths(2).ToPeriod());
        result.ScheduledPayments[0].ScheduledAmount.Should().Be(100m);
        result.ScheduledPayments[0].FinalPayment.Should().Be(0);
        
        result.ScheduledPayments[1].SourcePeriod.Should().Be(now.AddMonths(2).ToPeriod());
        result.ScheduledPayments[1].PaymentPeriod.Should().Be(now.AddMonths(3).ToPeriod());
        result.ScheduledPayments[1].ScheduledAmount.Should().Be(100m);
        result.ScheduledPayments[1].FinalPayment.Should().Be(0);
        
        result.ScheduledPayments[2].SourcePeriod.Should().Be(now.AddMonths(3).ToPeriod());
        result.ScheduledPayments[2].PaymentPeriod.Should().Be(now.AddMonths(4).ToPeriod());
        result.ScheduledPayments[2].ScheduledAmount.Should().Be(100m);
        result.ScheduledPayments[2].FinalPayment.Should().Be(75m);
    }
}