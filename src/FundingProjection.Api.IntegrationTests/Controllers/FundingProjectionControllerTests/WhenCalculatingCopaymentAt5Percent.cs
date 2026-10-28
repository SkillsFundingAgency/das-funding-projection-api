using FundingProjection.Api.Testing;
using FundingProjection.Api.Testing.DataCreation;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace FundingProjection.Api.IntegrationTests.Controllers.FundingProjectionControllerTests;

public class WhenCalculatingCopaymentAt5Percent: SqlBaseFixture
{
    public static readonly object[][] TestData = [
        [new DateOnly(2019, 3, 30), 0m, 0m],
        [new DateOnly(2019, 4, 1), 5m, 7.5m],
        [new DateOnly(2021, 6, 15), 5m, 7.5m],
        [new DateOnly(2026, 7, 31), 5m, 7.5m],
        [new DateOnly(2026, 8, 1), 0m, 0m],
    ];
    
    [TestCaseSource(nameof(TestData))]
    public async Task Then_It_Is_Calculated_Correctly(
        DateOnly startDate,
        decimal month1Amount,
        decimal month2Amount)
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        await DbData.AddManyAsync([
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x.Start(startDate).Length(2).TotalCost(250))
        ]);
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create(3, 0, 0, startDate));
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.Projections.Should().HaveCount(3);
        timeline.Projections[0].CoInvestmentCosts.Should().Be(0m);
        timeline.Projections[1].CoInvestmentCosts.Should().Be(month1Amount);
        timeline.Projections[2].CoInvestmentCosts.Should().Be(month2Amount);
    }
}