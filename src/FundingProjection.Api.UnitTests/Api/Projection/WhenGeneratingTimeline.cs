using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Projection;

public class WhenGeneratingTimeline
{
    [Test, MoqAutoData]
    public void Then_Only_Apprenticeships_In_The_Correct_Status_Are_Projected(
        long accountId,
        [Greedy] TimelineGenerator sut)
    {
        // arrange
        var now = DateOnly.FromDateTime(DateTime.UtcNow).ToPeriod();
        List<ApprenticeshipPaymentSummaryEntity> summaries =
        [
            .. new[] { ApprenticeshipStatus.Live, ApprenticeshipStatus.Paused, ApprenticeshipStatus.Stopped, ApprenticeshipStatus.Completed, ApprenticeshipStatus.Unknown }
                .Select((status, i) => new ApprenticeshipPaymentSummaryEntity
                {
                    ApprenticeshipId = i + 1,
                    AccountId = accountId,
                    TotalCost = 375m,
                    TotalPaid = 0m,
                    StartDate = now.AddMonths(-1),
                    EndDate = now.AddMonths(1),
                    Status = status,
                    IsTransfer = false,
                })
        ];

        var data = new TimelineData
        {
            AccountId = accountId,
            Months = 3,
            OpeningBalance = 0,
            StartingPeriod = now,
            ApprenticeshipPaymentSummaries = summaries,
        };

        // act
        var timeline = sut.Create(data);

        // assert
        timeline.Projections.Should().AllSatisfy(x => x.LevyOutSummary.CommittedLearnerCost.Should().Be(200m));
        timeline.Projections[^1].LevyOutSummary.CommittedLearnerFinalPaymentCost.Should().Be(150m);
    }
    
    [Test, MoqAutoData]
    public void Then_Only_Transferred_Apprenticeships_In_The_Correct_Status_Are_Projected(
        long accountId,
        [Greedy] TimelineGenerator sut)
    {
        // arrange
        var now = DateOnly.FromDateTime(DateTime.UtcNow).ToPeriod();
        List<ApprenticeshipPaymentSummaryEntity> summaries =
        [
            .. new[] { ApprenticeshipStatus.Live, ApprenticeshipStatus.Paused, ApprenticeshipStatus.Stopped, ApprenticeshipStatus.Completed, ApprenticeshipStatus.Unknown }
                .Select((status, i) => new ApprenticeshipPaymentSummaryEntity
                {
                    ApprenticeshipId = i + 1,
                    AccountId = accountId,
                    TotalCost = 375m,
                    TotalPaid = 0m,
                    StartDate = now.AddMonths(-1),
                    EndDate = now.AddMonths(1),
                    Status = status,
                    IsTransfer = true,
                })
        ];

        var data = new TimelineData
        {
            AccountId = accountId,
            Months = 3,
            OpeningBalance = 0,
            StartingPeriod = now,
            ApprenticeshipPaymentSummaries = summaries,
        };

        // act
        var timeline = sut.Create(data);

        // assert
        timeline.Projections.Should().AllSatisfy(x => x.LevyOutSummary.CommittedTransferOut.Should().Be(200m));
        timeline.Projections[^1].LevyOutSummary.CommittedTransferOutFinalPaymentCost.Should().Be(150m);
    }
}