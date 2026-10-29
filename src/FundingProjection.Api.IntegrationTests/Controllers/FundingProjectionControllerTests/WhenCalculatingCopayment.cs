using System.Collections.Generic;
using FundingProjection.Api.Testing;
using FundingProjection.Api.Testing.DataCreation;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace FundingProjection.Api.IntegrationTests.Controllers.FundingProjectionControllerTests;

public class WhenCalculatingCopayment: SqlBaseFixture
{
    [Test]
    public async Task Then_If_Start_Balance_Is_Positive_Then_The_Coinvestment_Starts_In_The_Correct_Period()
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        await DbData.AddManyAsync([
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x.Length(3).TotalCost(375).Age(25))
        ]);
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create(4, 100, 0));
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.Projections.Should().HaveCount(4);
        timeline.Projections[0].CoInvestmentCosts.Should().Be(0m);
        timeline.Projections[1].CoInvestmentCosts.Should().Be(0m);
        timeline.Projections[2].CoInvestmentCosts.Should().Be(25m);
        timeline.Projections[3].CoInvestmentCosts.Should().Be(43.75m);
    }
    
    [Test]
    public async Task Then_Copayment_Is_Only_Calculated_Against_The_Partial_Amount_That_Causes_A_Negative_Balance()
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        await DbData.AddManyAsync([
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x.Length(3).TotalCost(375).Age(25))
        ]);
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create(4, 25, 0));
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.Projections.Should().HaveCount(4);
        timeline.Projections[0].CoInvestmentCosts.Should().Be(0m);
        timeline.Projections[1].CoInvestmentCosts.Should().Be(18.75m);
        timeline.Projections[2].CoInvestmentCosts.Should().Be(25m);
        timeline.Projections[3].CoInvestmentCosts.Should().Be(43.75m);
    }
    
    [Test]
    public async Task Then_Employer_Can_Move_In_And_Out_Of_Coinvestment()
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        await DbData.AddManyAsync([
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x.Length(3).TotalCost(375).Age(25)),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x.StartsIn(months: 4).Length(3).TotalCost(375).Age(25))
        ]);
        var historicLevy = new List<LevyInMonthSummary>
        {
            new(DateTime.UtcNow.ToPeriod().AddMonths(3), 475)
        };
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create(9, 0, historicLevy));
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.Projections.Should().HaveCount(9);
        timeline.Projections[0].CoInvestmentCosts.Should().Be(0m);
        timeline.Projections[1].CoInvestmentCosts.Should().Be(25m);
        timeline.Projections[2].CoInvestmentCosts.Should().Be(25m);
        timeline.Projections[3].CoInvestmentCosts.Should().Be(43.75m);
        timeline.Projections[4].CoInvestmentCosts.Should().Be(0m);
        timeline.Projections[5].CoInvestmentCosts.Should().Be(0m); // the levy in covers this payment
        timeline.Projections[6].CoInvestmentCosts.Should().Be(25m); // back into co-investment
        timeline.Projections[7].CoInvestmentCosts.Should().Be(43.75m);
        timeline.Projections[8].CoInvestmentCosts.Should().Be(0m);
    }
    
    [Test]
    public async Task Then_Transferred_Apprenticeships_Are_Processed_First()
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        await DbData.AddManyAsync([
            // transferred must have a different rate than the other records 
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x.Start(new DateOnly(2026, 8, 1)).Age(25)),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x.Start(new DateOnly(2026, 7, 1)).Transferred()),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x.Start(new DateOnly(2026, 8, 1)).Age(25)),
        ]);
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create(9, 100, 100, new DateOnly(2026, 8, 1)));
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.Projections.Should().HaveCount(9);
        timeline.Projections[0].CoInvestmentCosts.Should().Be(0m);
        timeline.Projections[1].CoInvestmentCosts.Should().Be(50m);
    }
}