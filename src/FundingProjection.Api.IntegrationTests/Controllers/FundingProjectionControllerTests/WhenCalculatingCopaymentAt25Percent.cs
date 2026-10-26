using FundingProjection.Api.Testing;
using FundingProjection.Api.Testing.DataCreation;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace FundingProjection.Api.IntegrationTests.Controllers.FundingProjectionControllerTests;

public class WhenCalculatingCopaymentAt25Percent: SqlBaseFixture
{
    [TestCase(25)]
    [TestCase(26)]
    [TestCase(27)]
    public async Task Then_If_Start_Date_From_1st_August_2026_And_Apprentice_Is_25_Or_Over_At_Start_Then_The_Coinvestment_Is_Calculated_Correctly(int age)
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        await DbData.AddManyAsync([
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x.Length(3).TotalCost(375).Age(age))
        ]);
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create(4, 0, 0));
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.Projections.Should().HaveCount(4);
        timeline.Projections[0].CoInvestmentCosts.Should().Be(0m);
        timeline.Projections[1].CoInvestmentCosts.Should().Be(25m);
        timeline.Projections[2].CoInvestmentCosts.Should().Be(25m);
        timeline.Projections[3].CoInvestmentCosts.Should().Be(43.75m);
    }
    
    [TestCase(24)]
    [TestCase(23)]
    [TestCase(22)]
    public async Task Then_If_Start_Date_From_1st_August_2026_And_Apprentice_Is_Under_25_At_Start_Then_The_Coinvestment_Is_Calculated_Correctly(int age)
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        await DbData.AddManyAsync([
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x.Length(3).TotalCost(375).Age(age))
        ]);
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create(4, 0, 0));
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.Projections.Should().HaveCount(4);
        timeline.Projections[0].CoInvestmentCosts.Should().Be(0m);
        timeline.Projections[1].CoInvestmentCosts.Should().NotBe(25m);
        timeline.Projections[2].CoInvestmentCosts.Should().NotBe(25m);
        timeline.Projections[3].CoInvestmentCosts.Should().NotBe(43.75m);
    }
}