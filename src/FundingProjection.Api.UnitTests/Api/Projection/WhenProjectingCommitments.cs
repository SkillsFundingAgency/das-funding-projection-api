using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Projection;

public class WhenProjectingCommitments
{
    [Test, MoqAutoData]
    public void Then_The_Specified_Number_Of_Periods_Should_Be_Returned_Regardless_Of_No_Input_Data()
    {
        // act
        var results = CommitmentsProjector.CreateProjection([], 6);

        // assert
        results.Should().HaveCount(6);
        results.Should().AllSatisfy(x => x.TotalMonthlyPayments.Should().Be(0));
        results.Should().AllSatisfy(x => x.TotalFinalPayments.Should().Be(0));

        var i = 0;
        foreach (var result in results)
        {
            result.Period.Should().Be(DateTime.UtcNow.AddMonths(i++).ToPeriod());
        }
    }
    
    [Test, MoqAutoData]
    public void Then_The_Specified_Number_Of_Periods_Should_Be_Returned_Even_If_The_Data_Extends_Beyond_The_Requested_Number()
    {
        // arrange
        var now = DateTime.UtcNow;
        List<ApprenticeshipPaymentSummaryEntity> summaries =
        [
            new()
            {
                ApprenticeshipId = 1,
                AccountId = 1,
                TotalCost = 375m,
                TotalPaid = 0m,
                StartDate = now.AddMonths(0),
                EndDate = now.AddMonths(20),
                LastPaymentDate = null,
                LastPaymentAmount = null,
                Status = ApprenticeshipStatus.Live,
            }
        ];
        
        // act
        var results = CommitmentsProjector.CreateProjection(summaries, 3);

        // assert
        results.Should().HaveCount(3);
        
        var i = 0;
        foreach (var result in results)
        {
            result.Period.Should().Be(DateTime.UtcNow.AddMonths(i++).ToPeriod());
        }
    }
    
    [Test, MoqAutoData]
    public void Then_The_Payments_Are_Returned_In_The_Periods()
    {
        // arrange
        var now = DateTime.UtcNow;
        List<ApprenticeshipPaymentSummaryEntity> summaries =
        [
            new()
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
            }
        ];
        
        // act
        var results = CommitmentsProjector.CreateProjection(summaries, 3);

        // assert
        results.Should().HaveCount(3);
        results.Should().AllSatisfy(x => x.TotalMonthlyPayments.Should().Be(100m));
        results[^1].TotalFinalPayments.Should().Be(75m);
    }
    
    [Test, MoqAutoData]
    public void Then_The_Total_Payments_Are_Returned_In_The_Periods()
    {
        // arrange
        var now = DateTime.UtcNow;
        List<ApprenticeshipPaymentSummaryEntity> summaries =
        [
            new()
            {
                ApprenticeshipId = 1,
                AccountId = 1,
                TotalCost = 750m,
                TotalPaid = 750m,
                StartDate = now.AddMonths(-3),
                EndDate = now.AddMonths(-1),
                LastPaymentDate = now,
                LastPaymentAmount = 350m,
                Status = ApprenticeshipStatus.Completed,
            },
            new()
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
            },
            new()
            {
                ApprenticeshipId = 2,
                AccountId = 1,
                TotalCost = 375m,
                TotalPaid = 0m,
                StartDate = now.AddMonths(0),
                EndDate = now.AddMonths(2),
                LastPaymentDate = null,
                LastPaymentAmount = null,
                Status = ApprenticeshipStatus.Live,
            }
        ];
        
        // act
        var results = CommitmentsProjector.CreateProjection(summaries, 3);

        // assert
        results.Should().HaveCount(3);
        results[0].TotalMonthlyPayments.Should().Be(300m);
        results[0].TotalFinalPayments.Should().Be(150m);
        results[1].TotalMonthlyPayments.Should().Be(200m);
        results[1].TotalFinalPayments.Should().Be(0m);
        results[2].TotalMonthlyPayments.Should().Be(200m);
        results[2].TotalFinalPayments.Should().Be(75m);
    }
}