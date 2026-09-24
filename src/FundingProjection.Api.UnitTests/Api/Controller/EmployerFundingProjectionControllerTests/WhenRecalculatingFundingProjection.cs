using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using SFA.DAS.FundingProjection.Api.Controllers;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Data.Models;
using SFA.DAS.FundingProjection.Data.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Controller.EmployerFundingProjectionControllerTests;

[TestFixture]
internal class WhenRecalculatingFundingProjection
{
    [Test, RecursiveMoqAutoData]
    public async Task Get_ReturnsOk_WhenFundingProjectionService_Returns(
        DateTime cutOffDateTime,
        RecalculatedResponse mockResponse,
        [Frozen] Mock<IFundingProjectionServices> fundingProjectionService,
        [Greedy] FundingProjectionController controller,
        CancellationToken token)
    {
        // Arrange
        fundingProjectionService.Setup(p => p.UpdateProjection(cutOffDateTime, token)).ReturnsAsync(mockResponse);

        // Act
        var result = await controller.RecalculateFundingProjection(cutOffDateTime, fundingProjectionService.Object, token);

        // Assert
        result.Should().BeOfType<Ok<RecalculateFundingProjectionResponse>>();
        var okResult = result as Ok<RecalculateFundingProjectionResponse>;
        okResult!.Value!.TotalRecordsInserted.Should().Be(mockResponse.TotalRecordsInserted);
        okResult!.Value!.TotalRecordsProcessed.Should().Be(mockResponse.TotalRecordsProcessed);
        okResult!.Value!.TotalRecordsUpdated.Should().Be(mockResponse.TotalRecordsUpdated);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Get_ReturnsInternalServerException_WhenException_Thrown(DateTime cutOffDateTime,
        RecalculatedResponse mockResponse,
        [Frozen] Mock<IFundingProjectionServices> fundingProjectionService,
        [Greedy] FundingProjectionController controller,
        CancellationToken token)
    {
        // Arrange
        fundingProjectionService.Setup(p => p.UpdateProjection(cutOffDateTime, token)).ThrowsAsync(new Exception());

        // Act
        var result = await controller.RecalculateFundingProjection(cutOffDateTime, fundingProjectionService.Object, token);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
    }
}