using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using SFA.DAS.FundingProjection.Api.Controllers;
using SFA.DAS.FundingProjection.Api.Models;
using SFA.DAS.FundingProjection.Api.Models.Mappers;
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
        EmployerFundingProjectionEntity mockResponse,
        [Frozen] Mock<IEmployerFundingProjectionRepository> provider,
        [Greedy] EmployerFundingProjectionController controller,
        CancellationToken token)
    {
        // Arrange
        provider.Setup(p => p.GetByEmployerAccountIdAsync(accountId, token)).ReturnsAsync(mockResponse);

        // Act
        var result = await controller.Get(accountId, token);

        // Assert
        result.Should().BeOfType<Ok<GetEmployerFundingProjectionResponse>>();
        var okResult = result as Ok<GetEmployerFundingProjectionResponse>;
        okResult!.Value.Should().BeEquivalentTo(mockResponse.ToGetResponse());
    }

    [Test, RecursiveMoqAutoData]
    public async Task Get_ReturnsNotFound_WhenEmployerFundingProjectionEntityDoesNotExist(long accountId,
        EmployerFundingProjectionEntity mockResponse,
        [Frozen] Mock<IEmployerFundingProjectionRepository> provider,
        [Greedy] EmployerFundingProjectionController controller,
        CancellationToken token)
    {
        // Arrange
        provider.Setup(p => p.GetByEmployerAccountIdAsync(accountId, token)).ReturnsAsync((EmployerFundingProjectionEntity)null!);

        // Act
        var result = await controller.Get(accountId, token);

        // Assert
        result.Should().BeOfType<NotFound>();
    }

    [Test, RecursiveMoqAutoData]
    public async Task Get_ReturnsInternalServerException_WhenException_Thrown(long accountId,
        EmployerFundingProjectionEntity mockResponse,
        [Frozen] Mock<IEmployerFundingProjectionRepository> provider,
        [Greedy] EmployerFundingProjectionController controller,
        CancellationToken token)
    {
        // Arrange
        provider.Setup(p => p.GetByEmployerAccountIdAsync(accountId, token)).ThrowsAsync(new Exception());

        // Act
        var result = await controller.Get(accountId, token);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
    }
}