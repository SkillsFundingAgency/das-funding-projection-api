using FundingProjection.Api.Testing;
using FundingProjection.Api.Testing.DataCreation;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace FundingProjection.Api.IntegrationTests.Controllers.FundingProjectionControllerTests;

public class WhenProjectingCommitmentsInArrears : SqlBaseFixture
{
    [Test, MoqAutoData]
    public async Task Then_Payments_Are_Made_In_Arrears(long accountId)
    {
        // arrange
        var entity = ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x.Started(monthsAgo: 0).Length(3).TotalCost(375));
        await DbData.AddAsync(entity);
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create(4));
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.Projections.Should().HaveCount(4);
        timeline.Projections[0].LevyOutSummary.CommittedLearnerCost.Should().Be(0);
        timeline.Projections[..^1].Should().AllSatisfy(x => x.LevyOutSummary.CommittedLearnerFinalPaymentCost.Should().Be(0));
        timeline.Projections[1..].Should().AllSatisfy(x => x.LevyOutSummary.CommittedLearnerCost.Should().Be(100));
        timeline.Projections[^1].LevyOutSummary.CommittedLearnerFinalPaymentCost.Should().Be(75);
    }
    
    [Test, MoqAutoData]
    public async Task Then_An_Apprenticeship_Completed_In_The_Previous_Period_Has_A_Final_Payment_In_The_Current_Period(long accountId)
    {
        // arrange
        var entity = ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
            .Started(monthsAgo: 3).Length(3).TotalCost(375).TotalPaid(375).LastPayment(175m, DateTime.UtcNow.ToPeriod()));
        await DbData.AddAsync(entity);
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create(1));
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.Projections.Should().HaveCount(1);
        timeline.Projections[0].LevyOutSummary.CommittedLearnerCost.Should().Be(100);
        timeline.Projections[0].LevyOutSummary.CommittedLearnerFinalPaymentCost.Should().Be(75);
    }
}