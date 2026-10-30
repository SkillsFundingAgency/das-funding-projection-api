using FundingProjection.Api.Testing;
using FundingProjection.Api.Testing.DataCreation;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace FundingProjection.Api.IntegrationTests.Controllers.FundingProjectionControllerTests;

public class WhenCalculatingCopaymentWithOrdering: SqlBaseFixture
{
    [Test]
    public async Task Then_Transferred_Apprenticeships_Are_Prioritised()
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        await DbData.AddManyAsync([
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
                .Provider(1)
                .Start(new DateOnly(2026, 8, 1))),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
                .Provider(1)
                .Start(new DateOnly(2026, 8, 1))
                .Age(25)
                .TotalCost(10000)
                .Transferred()),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
                .Provider(1)
                .Start(new DateOnly(2026, 7, 31))
                .TotalPaid(100)
                .LastPayment(100, new DateOnly(2026, 8, 1))
                .Age(25)),
        ]);
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create(1, 1000, 0, new DateOnly(2026, 9, 1)));
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.Projections.Should().HaveCount(1);
        timeline.Projections[0].CoInvestmentCosts.Should().Be(5m);
    }
    
    [Test]
    public async Task Then_LearningType_Is_Used_To_Prioritise()
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        await DbData.AddManyAsync([
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
                .Provider(1)
                .Start(new DateOnly(2026, 8, 1))),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
                .Provider(2)
                .Start(new DateOnly(2026, 7, 31))
                .TotalPaid(100)
                .LastPayment(100, new DateOnly(2026, 8, 1))
                .Transferred()
                .ApprenticeshipUnit()),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
                .Provider(1)
                .Start(new DateOnly(2026, 8, 1))
                .Age(25)
                .TotalCost(10000)
                .Transferred()),
        ]);
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create(1, 1000, 0, new DateOnly(2026, 9, 1)));
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.Projections.Should().HaveCount(1);
        timeline.Projections[0].CoInvestmentCosts.Should().Be(5m);
    }
    
    [Test]
    public async Task Then_First_Approved_Provider_Apprenticeship_Is_Used_To_Prioritise()
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        await DbData.AddManyAsync([
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
                .Provider(1)
                .Start(new DateOnly(2026, 8, 1))),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
                .Provider(1)
                .Start(new DateOnly(2026, 7, 31))
                .TotalPaid(100)
                .LastPayment(100, new DateOnly(2026, 8, 1))
                .Approved(new DateTime(2026, 7, 4))),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
                .Provider(1)
                .Start(new DateOnly(2026, 8, 1))
                .Age(25)
                .TotalCost(10000)
                .Approved(new DateTime(2026, 7, 1))),
        ]);
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create(1, 1000, 0, new DateOnly(2026, 9, 1)));
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.Projections.Should().HaveCount(1);
        timeline.Projections[0].CoInvestmentCosts.Should().Be(5m);
    }
    
    [Test]
    public async Task Then_ProviderOrder_Ascending_Is_Used_To_Prioritise()
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        await DbData.AddManyAsync([
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
                .Provider(1)
                .Start(new DateOnly(2026, 8, 1))),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
                .Provider(1)
                .Start(new DateOnly(2026, 7, 31))
                .TotalPaid(100)
                .LastPayment(100, new DateOnly(2026, 8, 1))
                .Priority(2)),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
                .Provider(1)
                .Start(new DateOnly(2026, 8, 1))
                .Age(25)
                .TotalCost(10000)
                .Transferred()
                .Priority(1)),
        ]);
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create(1, 1000, 0, new DateOnly(2026, 9, 1)));
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.Projections.Should().HaveCount(1);
        timeline.Projections[0].CoInvestmentCosts.Should().Be(5m);
    }
    
    [Test]
    public async Task Then_ApprovalDate_Ascending_Is_Used_To_Prioritise()
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        await DbData.AddManyAsync([
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
                .Provider(1)
                .Start(new DateOnly(2026, 8, 1))),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
                .Provider(1)
                .Start(new DateOnly(2026, 7, 31))
                .TotalPaid(100)
                .LastPayment(100, new DateOnly(2026, 8, 1))
                .Priority(2)),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
                .Provider(1)
                .Start(new DateOnly(2026, 8, 1))
                .Age(25)
                .TotalCost(10000)
                .Transferred()
                .Priority(1)),
        ]);
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create(1, 1000, 0, new DateOnly(2026, 9, 1)));
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.Projections.Should().HaveCount(1);
        timeline.Projections[0].CoInvestmentCosts.Should().Be(5m);
    }
    
    [Test]
    public async Task Then_Uln_Ascending_Is_Used_To_Prioritise()
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        await DbData.AddManyAsync([
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
                .Provider(1)
                .Start(new DateOnly(2026, 8, 1))),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
                .Provider(2)
                .Start(new DateOnly(2026, 8, 1))
                .Age(25)
                .Approved(new DateTime(2026, 7, 20))
                .TotalCost(10000)),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
                .Provider(1)
                .Start(new DateOnly(2026, 7, 31))
                .Approved(new DateTime(2026, 7, 20))
                .TotalPaid(100)
                .LastPayment(100, new DateOnly(2026, 8, 1))),
        ]);
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create(1, 1000, 0, new DateOnly(2026, 9, 1)));
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.Projections.Should().HaveCount(1);
        timeline.Projections[0].CoInvestmentCosts.Should().Be(5m);
    }
}