using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using SFA.DAS.FundingProjection.Api.Controllers;
using SFA.DAS.FundingProjection.Api.Models.Mappers;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Data.Repositories;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.Testing.AutoFixture;

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
        const int months = 8;
        repository
            .Setup(x => x.GetApprenticeshipSummariesAsync(accountId))
            .ReturnsAsync([]);

        // act
        var result = await controller.GetEmployerFundingProjection(accountId, repository.Object, months, token) as Ok<GetEmployerFundingProjectionByAccountIdResponse>;

        // assert
        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value.FundingBreakdowns.Should().HaveCount(months);
        result.Value.FundingBreakdowns.Should().AllSatisfy(x => {
            x.EmployerAccountId.Should().Be(accountId);
            x.CommittedLearnerCost.Should().Be(0);
            x.CommittedLearnerFinalPaymentCost.Should().Be(0);
            x.CommittedTransferOut.Should().Be(0);
        });
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_A_Problem_Is_Returned_If_An_Exception_Occurs(long accountId,
        List<EmployerFundingProjectionEntity> mockResponse,
        [Frozen] Mock<IEmployerFundingProjectionRepository> repository,
        [Greedy] FundingProjectionController controller,
        CancellationToken token)
    {
        // arrange
        const int months = 8;
        repository
            .Setup(x => x.GetApprenticeshipSummariesAsync(accountId))
            .ThrowsAsync(new Exception());

        // act
        var result = await controller.GetEmployerFundingProjection(accountId, repository.Object, months, token);

        // assert
        result.Should().BeOfType<ProblemHttpResult>();
    }
}