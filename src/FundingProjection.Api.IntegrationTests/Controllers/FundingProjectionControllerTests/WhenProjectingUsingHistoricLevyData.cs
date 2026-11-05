using System.Linq;
using FundingProjection.Api.Testing;
using FundingProjection.Api.Testing.DataCreation;
using SFA.DAS.FundingProjection.Api.Models.Responses;

namespace FundingProjection.Api.IntegrationTests.Controllers.FundingProjectionControllerTests;

public class WhenProjectingUsingHistoricLevyData : SqlBaseFixture
{
    [Test]
    public async Task Then_With_No_Historic_Data_The_Timeline_Levy_In_Values_Are_Zero()
    {
        // act
        var accountId = AccountIdGenerator.Next();
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create(6, 0, []));
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.AccountId.Should().Be(accountId);
        timeline.TotalEstimatedCommitedCost.Should().Be(0);
        timeline.TotalEstimatedCommitedTransferCost.Should().Be(0);
        timeline.Projections.Count.Should().Be(6);
        timeline.Projections.Should().AllSatisfy(x =>
        {
            x.ClosingBalance.Should().Be(0);
            x.EmployerAccountId.Should().Be(accountId);
            x.LevyInSummary.LevyIn.Should().Be(0);
            x.LevyOutSummary.CommittedLearnerCost.Should().Be(0);
            x.LevyOutSummary.CommittedLearnerFinalPaymentCost.Should().Be(0);
            x.LevyOutSummary.CommittedTransferOut.Should().Be(0);
            x.LevyOutSummary.CommittedTransferOutFinalPaymentCost.Should().Be(0);
        });
    }
    
    [Test]
    public async Task Then_The_Provided_Historic_Levy_Is_Used_To_Project_The_Timeline()
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        var request = ProjectionRequest.Create(12);
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), request);
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.AccountId.Should().Be(accountId);
        timeline.TotalEstimatedCommitedCost.Should().Be(0);
        timeline.TotalEstimatedCommitedTransferCost.Should().Be(0);
        timeline.Projections.Count.Should().Be(request.Months);
        timeline.Projections.Should().AllSatisfy(x =>
        {
            var expectedLevyInPeriod = request.HistoricLevyIn.Find(c => c.Period.Month == x.Month && c.Period.Year == x.Year-1)!;
            x.LevyInSummary.LevyIn.Should().Be(expectedLevyInPeriod.Amount);
        });
    }
    
    [Test]
    public async Task Then_The_Provided_Historic_Levy_Is_Used_To_Project_The_Timeline_For_Multiple_Years()
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        var request = ProjectionRequest.Create(36);
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), request);
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.AccountId.Should().Be(accountId);
        timeline.TotalEstimatedCommitedCost.Should().Be(0);
        timeline.TotalEstimatedCommitedTransferCost.Should().Be(0);
        timeline.Projections.Count.Should().Be(request.Months);
        
        timeline.Projections.Should().AllSatisfy(x =>
        {
            int offset = timeline.Projections.IndexOf(x) / request.HistoricLevyIn.Count + 1;
            var expectedLevyInPeriod = request.HistoricLevyIn.Find(c => c.Period.Month == x.Month && c.Period.Year == x.Year-offset)!;
            x.LevyInSummary.LevyIn.Should().Be(expectedLevyInPeriod.Amount);
        });
    }
    
    [Test]
    public async Task Then_If_The_Historic_Levy_Contains_Levy_In_For_The_Current_Period_It_Is_Used_Instead_Of_The_12_Months_Prior_Levy_In_Figure()
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        var request = ProjectionRequest.Create(9, 1000m, HistoricLevy.Generate(includeCurrentMonth: true));
        var latestLevyIn = request.HistoricLevyIn.MaxBy(x => x.Period)!;
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), request);
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.AccountId.Should().Be(accountId);
        timeline.Projections.Count.Should().Be(request.Months);
        timeline.Projections[0].LevyInSummary.LevyIn.Should().Be(latestLevyIn.Amount);
    }
}