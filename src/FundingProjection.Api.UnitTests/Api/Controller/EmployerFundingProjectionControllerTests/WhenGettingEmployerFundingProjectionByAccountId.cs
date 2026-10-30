using Microsoft.AspNetCore.Http.HttpResults;
using SFA.DAS.EmployerFinance.Types.Models;
using SFA.DAS.FundingProjection.Api.Controllers;
using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Api.Projection;
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
            importJobStateRepository.Object, eventService.Object, new ExpiredFunds(), accountId, request, token) as Ok<EstimatesTimeline>;

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
        var result = await controller.PostEmployerFundingProjection(apprenticeshipSummariesRepository.Object, importJobStateRepository.Object, eventService.Object, new ExpiredFunds(), accountId, request, token) as Ok<EstimatesTimeline>;

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
        var result = await controller.PostEmployerFundingProjection(apprenticeshipSummariesRepository.Object, importJobStateRepository.Object, eventService.Object, new ExpiredFunds(), accountId, request, token) as Ok<EstimatesTimeline>;

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
        var result = await controller.PostEmployerFundingProjection(apprenticeshipSummariesRepository.Object, importJobStateRepository.Object, eventService.Object, new ExpiredFunds(), accountId, request, token) as Ok<EstimatesTimeline>;

        // assert
        result.Should().NotBeNull();
        result.Value!.TotalEstimatedCommitedCost.Should().Be(275m + 1500m + 1000m);
        result.Value.TotalEstimatedCommitedTransferCost.Should().Be(550m + 600m + 800m);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_All_Levy_Expires_After_Its_Expiry_Period_When_There_Are_No_Estimated_Values(
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
            .ReturnsAsync([]);

        importJobStateRepository
            .Setup(x => x.GetOrCreateJobStateAsync(accountId, JobName.ImportLearnersCost, token))
            .ReturnsAsync(importJobState);

        eventService.Setup(x => x.HandleImportLearnersCost(importJobState.Id, accountId)).Returns(Task.CompletedTask);

        // levy received before August 2026 expires after 24 months, and from then after 12 months
        var policyChange = new DateOnly(2026, 8, 1);
        var levyFundsIn = Enumerable.Range(-24, 24)
            .Select(i => now.ToPeriod().AddMonths(i))
            .Select(p => new CalendarPeriodAmount { Year = p.Year, Month = p.Month, Amount = p.Month * 100m })
            .ToList();
        var totalLevyIn = levyFundsIn.Sum(x => x.Amount);

        var request = new PostEmployerFundingProjectionRequest
        {
            Months = 24,
            CurrentLevyFunds = totalLevyIn,
            HistoricLevyIn = [],
            LevyFundsInForExpiry = levyFundsIn
        };

        // act
        var result = await controller.PostEmployerFundingProjection(apprenticeshipSummariesRepository.Object, importJobStateRepository.Object, eventService.Object, new ExpiredFunds(), accountId, request, token) as Ok<EstimatesTimeline>;

        // assert
        result.Should().NotBeNull();
        var projections = result.Value!.Projections;
        projections.Should().HaveCount(24);
        foreach (var projection in projections)
        {
            var period = new DateOnly(projection.Year, projection.Month, 1);
            var expectedExpiry = levyFundsIn
                .Where(x =>
                {
                    var received = new DateOnly(x.Year, x.Month, 1);
                    return received.AddMonths(received < policyChange ? 24 : 12) == period;
                })
                .Sum(x => x.Amount);
            projection.LevyOutSummary.ExpiredLevy.Should().Be(expectedExpiry);
        }
        projections.Sum(x => x.LevyOutSummary.ExpiredLevy).Should().Be(totalLevyIn);
        projections[^1].ClosingBalance.Should().Be(0m);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_No_Levy_Expires_When_The_Estimated_Spend_Matches_The_Levy_In_From_12_Months_Ago(
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
        var levyFundsIn = Enumerable.Range(-12, 12)
            .Select(i => now.AddMonths(i))
            .Select(p => new CalendarPeriodAmount { Year = p.Year, Month = p.Month, Amount = p.Month * 100m })
            .ToList();
        var totalLevyIn = levyFundsIn.Sum(x => x.Amount);

        // one apprenticeship per projected month, each paid in full in that month (instalment plus completion),
        // costing the same as the levy that came in 12 months earlier
        apprenticeshipSummariesRepository
            .Setup(x => x.GetApprenticeshipPaymentSummariesAsync(accountId, token))
            .ReturnsAsync(levyFundsIn.Select((levyIn, i) => new ApprenticeshipPaymentSummaryEntity
            {
                ApprenticeshipId = i + 1,
                AccountId = accountId,
                IsTransfer = false,
                TotalCost = levyIn.Amount,
                TotalPaid = 0m,
                StartDate = now.AddMonths(i - 1),
                EndDate = now.AddMonths(i - 1),
                Status = i == 0 ? ApprenticeshipStatus.Live : ApprenticeshipStatus.WaitingToStart,
            }).ToList());

        importJobStateRepository
            .Setup(x => x.GetOrCreateJobStateAsync(accountId, JobName.ImportLearnersCost, token))
            .ReturnsAsync(importJobState);

        eventService.Setup(x => x.HandleImportLearnersCost(importJobState.Id, accountId)).Returns(Task.CompletedTask);

        var request = new PostEmployerFundingProjectionRequest
        {
            Months = 12,
            CurrentLevyFunds = totalLevyIn,
            HistoricLevyIn = [],
            LevyFundsInForExpiry = levyFundsIn
        };

        // act
        var result = await controller.PostEmployerFundingProjection(apprenticeshipSummariesRepository.Object, importJobStateRepository.Object, eventService.Object, new ExpiredFunds(), accountId, request, token) as Ok<EstimatesTimeline>;

        // assert
        result.Should().NotBeNull();
        var projections = result.Value!.Projections;
        projections.Should().HaveCount(12);
        projections.TrueForAll(x => x.LevyOutSummary.ExpiredLevy == 0).Should().BeTrue();
        projections[^1].ClosingBalance.Should().Be(0m);
    }
}
