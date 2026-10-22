using FundingProjection.Api.Testing;
using FundingProjection.Api.Testing.DataCreation;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace FundingProjection.Api.IntegrationTests.Controllers.FundingProjectionControllerTests;

public class WhenCalculatingSummaryTotals: SqlBaseFixture
{
    public static readonly object[][] WhenCalculatingSummaryTotalsData = [
        [0, 0m, 1000m],
        [1, 100m, 900m],
        [2, 200m, 800m],
        [3, 300m, 700m],
        [4, 400m, 600m],
        [5, 500m, 500m],
        [6, 600m, 400m],
        [7, 700m, 300m],
        [8, 800m, 0m],
    ];

    [Test]
    [TestCaseSource(nameof(WhenCalculatingSummaryTotalsData))]
    public async Task Then_Total_Committed_Costs_Is_Reduced_As_The_Apprenticeship_Payments_Are_Made(
        int monthsAgo,
        decimal totalPaid,
        decimal expectedTotal)
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        await DbData.AddManyAsync([
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x.Started(monthsAgo).TotalPaid(totalPaid)),
        ]);
        
        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create());
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        timeline.TotalEstimatedCommitedCost.Should().Be(expectedTotal);
        timeline.TotalEstimatedCommitedTransferCost.Should().Be(0);
    }
}