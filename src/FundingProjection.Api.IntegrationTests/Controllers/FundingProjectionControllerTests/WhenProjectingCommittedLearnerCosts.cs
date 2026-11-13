using FundingProjection.Api.Testing;
using FundingProjection.Api.Testing.DataCreation;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace FundingProjection.Api.IntegrationTests.Controllers.FundingProjectionControllerTests;

public class WhenProjectingCommittedLearnerCosts : SqlBaseFixture
{
    [Test]
    public async Task Then_Multiple_Apprenticeship_Commitment_Monthly_Payments_Are_Summed_For_Each_Period()
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        await DbData.AddManyAsync([
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x.Started(monthsAgo: 1).Length(3).TotalCost(750)), 
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x.Started(monthsAgo: 0).Length(3).TotalCost(375)), 
        ]);
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create(4));
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.TotalEstimatedCommitedCost.Should().Be(1125);
        timeline.TotalEstimatedCommitedTransferCost.Should().Be(0);
        timeline.Projections.Should().HaveCount(4);
        timeline.Projections[0].LevyOutSummary.CommittedLearnerCost.Should().Be(200);
        timeline.Projections[1].LevyOutSummary.CommittedLearnerCost.Should().Be(300);
        timeline.Projections[2].LevyOutSummary.CommittedLearnerCost.Should().Be(300);
        timeline.Projections[3].LevyOutSummary.CommittedLearnerCost.Should().Be(100);
    }
    
    [Test]
    public async Task Then_Multiple_Apprenticeship_Commitment_Final_Payments_Are_Summed_For_Each_Period()
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        await DbData.AddManyAsync([
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x.Started(monthsAgo: 0).Length(3).TotalCost(750)), 
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x.Started(monthsAgo: 0).Length(3).TotalCost(375)), 
        ]);
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create(4));
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.TotalEstimatedCommitedCost.Should().Be(1125);
        timeline.TotalEstimatedCommitedTransferCost.Should().Be(0);
        timeline.Projections.Should().HaveCount(4);
        timeline.Projections[^1].LevyOutSummary.CommittedLearnerFinalPaymentCost.Should().Be(225);
    }
    
    [Test]
    public async Task Then_Completed_Apprenticeships_Are_Not_Included_In_The_Projections_When_They_Finished_More_Than_2_Months_Ago()
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        await DbData.AddManyAsync([
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x.Started(monthsAgo: 6).Length(3).TotalCost(750)), 
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x.Started(monthsAgo: 6).Length(3).TotalCost(375)), 
        ]);
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create(4));
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.TotalEstimatedCommitedCost.Should().Be(0);
        timeline.TotalEstimatedCommitedTransferCost.Should().Be(0);
        timeline.Projections.Should().HaveCount(4);
        timeline.Projections.Should().AllSatisfy(x => x.LevyOutSummary.CommittedLearnerFinalPaymentCost.Should().Be(0));
    }
    
    [Test]
    public async Task Then_Stopped_Apprenticeships_Are_Not_Included_In_The_Results()
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        await DbData.AddManyAsync([
            ApprenticeshipPaymentSummaryEntity.Create(accountId),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x.Started(monthsAgo: 4).Stopped()), 
        ]);
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create(9));
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.TotalEstimatedCommitedCost.Should().Be(TestData.DefaultTotalCost);
        timeline.TotalEstimatedCommitedTransferCost.Should().Be(0);
        timeline.Projections.Should().HaveCount(9);
        timeline.Projections[1..].Should().AllSatisfy(x => x.LevyOutSummary.CommittedLearnerCost.Should().Be(TestData.DefaultMonthlyCommitment));
        timeline.Projections[^1].LevyOutSummary.CommittedLearnerFinalPaymentCost.Should().Be(TestData.DefaultFinalPayment);
    }
    
    [Test]
    public async Task Then_Paused_Apprenticeships_Are_Not_Included_In_The_Projected_Commitments_But_Are_Included_In_The_Totals()
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        await DbData.AddManyAsync([
            ApprenticeshipPaymentSummaryEntity.Create(accountId),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x.Started(monthsAgo: 4).TotalPaid(300m).LastPayment(100m, DateTime.UtcNow.ToPeriod()).Paused()), 
        ]);
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create(9));
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.TotalEstimatedCommitedCost.Should().Be(2 * TestData.DefaultTotalCost - 300m);
        timeline.TotalEstimatedCommitedTransferCost.Should().Be(0);
        timeline.Projections.Should().HaveCount(9);
        timeline.Projections[1..].Should().AllSatisfy(x => x.LevyOutSummary.CommittedLearnerCost.Should().Be(TestData.DefaultMonthlyCommitment));
        timeline.Projections[^1].LevyOutSummary.CommittedLearnerFinalPaymentCost.Should().Be(TestData.DefaultFinalPayment);
    }
}