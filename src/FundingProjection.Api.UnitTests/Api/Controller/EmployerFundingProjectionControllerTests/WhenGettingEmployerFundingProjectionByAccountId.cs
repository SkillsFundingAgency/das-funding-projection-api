using Microsoft.AspNetCore.Http.HttpResults;
using SFA.DAS.FundingProjection.Api.Controllers;
using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Data.Repositories;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Controller.EmployerFundingProjectionControllerTests;

[TestFixture]
internal class WhenGettingEmployerFundingProjectionByAccountId
{
    [Test, RecursiveMoqAutoData]
    public async Task Then_The_Projection_Is_Returned_Even_When_There_Is_No_Data(
        long accountId,
        [Frozen] Mock<IEmployerFundingProjectionRepository> repository,
        [Greedy] FundingProjectionController controller,
        CancellationToken token)
    {
        // arrange
        repository
            .Setup(x => x.GetApprenticeshipSummariesAsync(accountId))
            .ReturnsAsync([]);
        
        var request = new PostEmployerFundingProjectionRequest()
        {
            Months = 8,
            HistoricLevyIn = []
        };

        // act
        var result = await controller.PostEmployerFundingProjection(repository.Object, accountId, request, token) as Ok<EstimatesTimeline>;

        // assert
        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value.AccountId.Should().Be(accountId);
        result.Value.Projections.Should().HaveCount(request.Months);
        result.Value.Projections.Should().AllSatisfy(x => {
            x.EmployerAccountId.Should().Be(accountId);
            x.ClosingBalance.Should().Be(0);
            x.LevyOutSummary.CommittedLearnerCost.Should().Be(0);
            x.LevyOutSummary.CommittedLearnerFinalPaymentCost.Should().Be(0);
            x.LevyOutSummary.CommittedTransferOut.Should().Be(0);
            x.LevyOutSummary.CommittedTransferOutFinalPaymentCost.Should().Be(0);
            x.LevyInSummary.LevyIn.Should().Be(0);
        });
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_Transfer_Costs_Are_Projected_Separately_From_Committed_Learner_Costs(
        long accountId,
        [Frozen] Mock<IEmployerFundingProjectionRepository> repository,
        [Greedy] FundingProjectionController controller,
        CancellationToken token)
    {
        // arrange
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        repository
            .Setup(x => x.GetApprenticeshipSummariesAsync(accountId))
            .ReturnsAsync([
                new ApprenticeshipPaymentSummaryEntity
                {
                    ApprenticeshipId = 1,
                    AccountId = accountId,
                    IsTransfer = false,
                    TotalCost = 375m,
                    TotalPaid = 0m,
                    StartDate = now.AddMonths(-1),
                    EndDate = now.AddMonths(1),
                    Status = ApprenticeshipStatus.Live,
                },
                new ApprenticeshipPaymentSummaryEntity
                {
                    ApprenticeshipId = 2,
                    AccountId = accountId,
                    IsTransfer = true,
                    TotalCost = 750m,
                    TotalPaid = 0m,
                    StartDate = now.AddMonths(-1),
                    EndDate = now.AddMonths(1),
                    Status = ApprenticeshipStatus.Live,
                }
            ]);

        var request = new PostEmployerFundingProjectionRequest()
        {
            Months = 3,
            HistoricLevyIn = []
        };

        // act
        var result = await controller.PostEmployerFundingProjection(repository.Object, accountId, request, token) as Ok<EstimatesTimeline>;

        // assert
        result.Should().NotBeNull();
        var projections = result.Value!.Projections;
        projections.Should().HaveCount(request.Months);
        projections.Should().AllSatisfy(x => {
            x.LevyOutSummary.CommittedLearnerCost.Should().Be(100m);
            x.LevyOutSummary.CommittedTransferOut.Should().Be(200m);
        });
        projections[0].LevyOutSummary.CommittedLearnerFinalPaymentCost.Should().Be(0m);
        projections[0].LevyOutSummary.CommittedTransferOutFinalPaymentCost.Should().Be(0m);
        projections[^1].LevyOutSummary.CommittedLearnerFinalPaymentCost.Should().Be(75m);
        projections[^1].LevyOutSummary.CommittedTransferOutFinalPaymentCost.Should().Be(150m);
        projections[^1].ClosingBalance.Should().Be(-1125m);
    }
}