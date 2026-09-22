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
    public async Task Get_ReturnsOk_WhenEmployerFundingProjectionEntityExists(
        long accountId,
        int months,
        List<EmployerFundingProjectionEntity> mockResponse,
        [Frozen] Mock<IEmployerFundingProjectionRepository> repository,
        [Greedy] FundingProjectionController controller,
        CancellationToken token)
    {
        // Arrange
        repository.Setup(p => p.GetTotalCostByMonthsAsync(accountId, months, token)).ReturnsAsync(mockResponse);

        // Act
        var result = await controller.GetEmployerFundingProjection(accountId, repository.Object, months, token);

        // Assert
        result.Should().BeOfType<Ok<IEnumerable<MonthlyFundingBreakdown>>>();
        var okResult = result as Ok<IEnumerable<MonthlyFundingBreakdown>>;
        okResult!.Value.Should().BeEquivalentTo(mockResponse.Select(x => x.ToGetResponse()));
    }

    [Test, RecursiveMoqAutoData]
    public async Task Get_ReturnsInternalServerException_WhenException_Thrown(long accountId,
        List<EmployerFundingProjectionEntity> mockResponse,
        [Frozen] Mock<IEmployerFundingProjectionRepository> repository,
        [Greedy] FundingProjectionController controller,
        CancellationToken token)
    {
        // Arrange
        repository.Setup(p => p.GetTotalCostByMonthsAsync(accountId, 12, token)).ThrowsAsync(new Exception());

        // Act
        var result = await controller.GetEmployerFundingProjection(accountId, repository.Object, 12, token);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
    }
}