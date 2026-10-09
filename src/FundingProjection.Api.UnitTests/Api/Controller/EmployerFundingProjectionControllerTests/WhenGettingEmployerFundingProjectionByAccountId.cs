using Microsoft.AspNetCore.Http.HttpResults;
using SFA.DAS.FundingProjection.Api.Controllers;
using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Data.Repositories;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Controller.EmployerFundingProjectionControllerTests;

[TestFixture]
internal class WhenGettingEmployerFundingProjectionByAccountId
{
    [Test, RecursiveMoqAutoData]
    public async Task Then_The_Projection_Is_Returned_Even_When_There_Is_No_Data(
        long accountId,
        [Frozen] Mock<IEmployerFundingProjectionRepository> repository,
        [Greedy] FundingProjectionController controller,
        CancellationToken token)
    {
        // arrange
        repository
            .Setup(x => x.GetApprenticeshipSummariesAsync(accountId))
            .ReturnsAsync([]);
        
        var request = new PostEmployerFundingProjectionRequest()
        {
            Months = 8,
            HistoricLevyIn = []
        };

        // act
        var result = await controller.PostEmployerFundingProjection(repository.Object, accountId, request, token) as Ok<EstimatesTimeline>;

        // assert
        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value.AccountId.Should().Be(accountId);
        result.Value.Projections.Should().HaveCount(request.Months);
        result.Value.Projections.Should().AllSatisfy(x => {
            x.EmployerAccountId.Should().Be(accountId);
            x.OpeningBalance.Should().Be(0);
            x.CommittedLearnerCost.Should().Be(0);
            x.CommittedLearnerFinalPaymentCost.Should().Be(0);
            x.CommittedTransferOut.Should().Be(0);
            x.LevyIn.Should().Be(0);
        });
    }
}