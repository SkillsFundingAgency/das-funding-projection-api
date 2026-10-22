using FundingProjection.Api.Testing;
using FundingProjection.Api.Testing.DataCreation;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace FundingProjection.Api.IntegrationTests.Controllers.FundingProjectionControllerTests;

public class WhenProjectingCommittedLearnerCosts : SqlBaseFixture
{
    [Test, MoqAutoData]
    public async Task Then_Multiple_Apprenticeship_Commitment_Monthly_Payments_Are_Summed_For_Each_Period(long accountId)
    {
        // arrange
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
        timeline.Projections.Should().HaveCount(4);
        timeline.Projections[0].LevyOutSummary.CommittedLearnerCost.Should().Be(200);
        timeline.Projections[1].LevyOutSummary.CommittedLearnerCost.Should().Be(300);
        timeline.Projections[2].LevyOutSummary.CommittedLearnerCost.Should().Be(300);
        timeline.Projections[3].LevyOutSummary.CommittedLearnerCost.Should().Be(100);
    }
    
    [Test, MoqAutoData]
    public async Task Then_Multiple_Apprenticeship_Commitment_Final_Payments_Are_Summed_For_Each_Period(long accountId)
    {
        // arrange
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
        timeline.Projections.Should().HaveCount(4);
        timeline.Projections[^1].LevyOutSummary.CommittedLearnerFinalPaymentCost.Should().Be(225);
    }
}