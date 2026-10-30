using FundingProjection.Api.Testing;
using FundingProjection.Api.Testing.DataCreation;
using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Api.Models.Responses;

namespace FundingProjection.Api.IntegrationTests.Controllers.FundingProjectionControllerTests;

public class WhenProjectingFromALiveBalance : SqlBaseFixture
{
    [Test]
    public async Task Then_Levy_That_Has_Already_Come_In_This_Month_Is_Not_Counted_Twice()
    {
        // arrange - the current balance already includes this month's 1000 levy
        var accountId = AccountIdGenerator.Next();
        var request = new PostEmployerFundingProjectionRequest
        {
            Months = 1,
            CurrentLevyFunds = 5000m,
            HistoricLevyIn = HistoricLevy.Generate(fixedMonthlyAmount: 1000m, includeCurrentMonth: true),
        };

        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), request);
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline!.Projections[0].LevyInSummary.LevyIn.Should().Be(1000m);
        timeline.Projections[0].ClosingBalance.Should().Be(5000m);
    }

    [Test]
    public async Task Then_Levy_Not_Yet_Come_In_This_Month_Is_Added_To_The_Current_Balance()
    {
        // arrange - this month's levy has not come in, so last year's is used as the estimate
        var accountId = AccountIdGenerator.Next();
        var request = new PostEmployerFundingProjectionRequest
        {
            Months = 1,
            CurrentLevyFunds = 5000m,
            HistoricLevyIn = HistoricLevy.Generate(fixedMonthlyAmount: 1000m),
        };

        // act
        var response = await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), request);
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline!.Projections[0].LevyInSummary.LevyIn.Should().Be(1000m);
        timeline.Projections[0].ClosingBalance.Should().Be(6000m);
    }
}
