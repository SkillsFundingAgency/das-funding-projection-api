using Microsoft.AspNetCore.Http.HttpResults;
using SFA.DAS.FundingProjection.Api.Controllers;
using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Data.Repositories;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;
using SFA.DAS.FundingProjection.Domain.Services;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Controller.EmployerFundingProjectionControllerTests;

[TestFixture]
internal class WhenGettingEmployerFundingProjectionByAccountId
{
    [Test, RecursiveMoqAutoData]
    public async Task Then_The_Projection_Is_Returned_Even_When_There_Is_No_Data(
        long accountId,
        ImportJobStateEntity importJobState,
        [Frozen] Mock<IApprenticeshipPaymentSummariesRepository> apprenticeshipSummariesRepository,
        [Frozen] Mock<IImportJobStateRepository> importJobStateRepository,
        [Frozen] Mock<IEventService> eventService,
        [Greedy] FundingProjectionController controller,
        CancellationToken token)
    {
        // arrange
        apprenticeshipSummariesRepository   
            .Setup(x => x.GetApprenticeshipPaymentSummariesAsync(accountId, token))
            .ReturnsAsync([]);

        importJobStateRepository
            .Setup(x => x.GetOrCreateJobStateAsync(accountId, JobName.ImportLearnersCost, token))
            .ReturnsAsync(importJobState);

        eventService.Setup(x => x.HandleImportLearnersCost(importJobState.Id, accountId)).Returns(Task.CompletedTask);

        var request = new PostEmployerFundingProjectionRequest
        {
            Months = 8,
            HistoricLevyIn = []
        };

        // act
        var result = await controller.PostEmployerFundingProjection(apprenticeshipSummariesRepository.Object,
            importJobStateRepository.Object, eventService.Object, accountId, request, token) as Ok<EstimatesTimeline>;

        // assert
        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value.AccountId.Should().Be(accountId);
        result.Value.TotalEstimatedCommitedCost.Should().Be(0);
        result.Value.TotalEstimatedCommitedTransferCost.Should().Be(0);
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
        ImportJobStateEntity importJobState,
        [Frozen] Mock<IApprenticeshipPaymentSummariesRepository> apprenticeshipSummariesRepository,
        [Frozen] Mock<IImportJobStateRepository> importJobStateRepository,
        [Frozen] Mock<IEventService> eventService,
        [Greedy] FundingProjectionController controller,
        CancellationToken token)
    {
        // arrange
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        apprenticeshipSummariesRepository
            .Setup(x => x.GetApprenticeshipPaymentSummariesAsync(accountId, token))
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
        importJobStateRepository
            .Setup(x => x.GetOrCreateJobStateAsync(accountId, JobName.ImportLearnersCost, token))
            .ReturnsAsync(importJobState);

        eventService.Setup(x => x.HandleImportLearnersCost(importJobState.Id, accountId)).Returns(Task.CompletedTask);

        var request = new PostEmployerFundingProjectionRequest()
        {
            Months = 3,
            HistoricLevyIn = []
        };

        // act
        var result = await controller.PostEmployerFundingProjection(apprenticeshipSummariesRepository.Object, importJobStateRepository.Object, eventService.Object, accountId, request, token) as Ok<EstimatesTimeline>;

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

    [Test, RecursiveMoqAutoData]
    public async Task Then_The_Estimated_Committed_Costs_Are_The_Remaining_Costs_Split_By_Transfer(
        long accountId,
        ImportJobStateEntity importJobState,
        [Frozen] Mock<IApprenticeshipPaymentSummariesRepository> apprenticeshipSummariesRepository,
        [Frozen] Mock<IImportJobStateRepository> importJobStateRepository,
        [Frozen] Mock<IEventService> eventService,
        [Greedy] FundingProjectionController controller,
        CancellationToken token)
    {
        // arrange
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        apprenticeshipSummariesRepository
            .Setup(x => x.GetApprenticeshipPaymentSummariesAsync(accountId, token))
            .ReturnsAsync([
                new ApprenticeshipPaymentSummaryEntity
                {
                    ApprenticeshipId = 1,
                    AccountId = accountId,
                    IsTransfer = false,
                    TotalCost = 375m,
                    TotalPaid = 100m,
                    StartDate = now.AddMonths(-2),
                    EndDate = now.AddMonths(1),
                    Status = ApprenticeshipStatus.Live,
                },
                new ApprenticeshipPaymentSummaryEntity
                {
                    ApprenticeshipId = 2,
                    AccountId = accountId,
                    IsTransfer = false,
                    TotalCost = 1500m,
                    TotalPaid = 0m,
                    StartDate = now,
                    EndDate = now.AddMonths(12),
                    Status = ApprenticeshipStatus.Live,
                },
                new ApprenticeshipPaymentSummaryEntity
                {
                    ApprenticeshipId = 3,
                    AccountId = accountId,
                    IsTransfer = true,
                    TotalCost = 750m,
                    TotalPaid = 200m,
                    StartDate = now.AddMonths(-2),
                    EndDate = now.AddMonths(1),
                    Status = ApprenticeshipStatus.Live,
                },
                new ApprenticeshipPaymentSummaryEntity
                {
                    ApprenticeshipId = 4,
                    AccountId = accountId,
                    IsTransfer = true,
                    TotalCost = 1000m,
                    TotalPaid = 1000m,
                    StartDate = now.AddMonths(-12),
                    EndDate = now.AddMonths(-1),
                    Status = ApprenticeshipStatus.Completed,
                }
            ]);
        importJobStateRepository
            .Setup(x => x.GetOrCreateJobStateAsync(accountId, JobName.ImportLearnersCost, token))
            .ReturnsAsync(importJobState);

        eventService.Setup(x => x.HandleImportLearnersCost(importJobState.Id, accountId)).Returns(Task.CompletedTask);

        var request = new PostEmployerFundingProjectionRequest
        {
            Months = 3,
            HistoricLevyIn = []
        };

        // act
        var result = await controller.PostEmployerFundingProjection(apprenticeshipSummariesRepository.Object, importJobStateRepository.Object, eventService.Object, accountId, request, token) as Ok<EstimatesTimeline>;

        // assert
        result.Should().NotBeNull();
        result.Value!.TotalEstimatedCommitedCost.Should().Be(275m + 1500m);
        result.Value.TotalEstimatedCommitedTransferCost.Should().Be(550m);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_The_Estimated_Committed_Costs_Only_Include_Apprenticeships_With_Remaining_Cost(
        long accountId,
        ImportJobStateEntity importJobState,
        [Frozen] Mock<IApprenticeshipPaymentSummariesRepository> apprenticeshipSummariesRepository,
        [Frozen] Mock<IImportJobStateRepository> importJobStateRepository,
        [Frozen] Mock<IEventService> eventService,
        [Greedy] FundingProjectionController controller,
        CancellationToken token)
    {
        // arrange
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        ApprenticeshipPaymentSummaryEntity Summary(long id, bool isTransfer, ApprenticeshipStatus status, decimal totalCost, decimal totalPaid) => new()
        {
            ApprenticeshipId = id,
            AccountId = accountId,
            IsTransfer = isTransfer,
            TotalCost = totalCost,
            TotalPaid = totalPaid,
            StartDate = now.AddMonths(-1),
            EndDate = now.AddMonths(6),
            Status = status,
        };

        apprenticeshipSummariesRepository
            .Setup(x => x.GetApprenticeshipPaymentSummariesAsync(accountId, token))
            .ReturnsAsync([
                // included
                Summary(1, false, ApprenticeshipStatus.Live, 375m, 100m),
                Summary(2, false, ApprenticeshipStatus.WaitingToStart, 1500m, 0m),
                Summary(3, false, ApprenticeshipStatus.Paused, 1000m, 0m),
                Summary(4, true, ApprenticeshipStatus.Live, 750m, 200m),
                Summary(5, true, ApprenticeshipStatus.WaitingToStart, 600m, 0m),
                Summary(6, true, ApprenticeshipStatus.Paused, 800m, 0m),
                // excluded
                Summary(7, false, ApprenticeshipStatus.Completed, 375m, 300m),
                Summary(8, false, ApprenticeshipStatus.Stopped, 2000m, 500m),
                Summary(9, false, ApprenticeshipStatus.Unknown, 3000m, 0m),
                Summary(10, true, ApprenticeshipStatus.Completed, 450m, 360m),
                Summary(11, true, ApprenticeshipStatus.Stopped, 900m, 100m),
            ]);
        importJobStateRepository
            .Setup(x => x.GetOrCreateJobStateAsync(accountId, JobName.ImportLearnersCost, token))
            .ReturnsAsync(importJobState);

        eventService.Setup(x => x.HandleImportLearnersCost(importJobState.Id, accountId)).Returns(Task.CompletedTask);

        var request = new PostEmployerFundingProjectionRequest
        {
            Months = 3,
            HistoricLevyIn = []
        };

        // act
        var result = await controller.PostEmployerFundingProjection(apprenticeshipSummariesRepository.Object, importJobStateRepository.Object, eventService.Object, accountId, request, token) as Ok<EstimatesTimeline>;

        // assert
        result.Should().NotBeNull();
        result.Value!.TotalEstimatedCommitedCost.Should().Be(275m + 1500m + 1000m);
        result.Value.TotalEstimatedCommitedTransferCost.Should().Be(550m + 600m + 800m);
    }
}
