using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using SFA.DAS.FundingProjection.Api.Controllers;
using SFA.DAS.FundingProjection.Api.Models.Mappers;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Data.Repositories;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Controller.JobsControllerTests;

[TestFixture]
internal class WhenGettingOrCreatingJobState
{
    [Test, RecursiveMoqAutoData]
    public async Task GetOrCreateJobState_ReturnsOk_WhenJobStateRetrievedSuccessfully(
        JobName jobName,
        ImportJobStateEntity mockJobState,
        [Frozen] Mock<IImportJobStateRepository> repository,
        [Greedy] JobsController endpoint,
        CancellationToken cancellationToken)
    {
        // Arrange
        repository.Setup(r => r.GetOrCreateJobStateAsync(jobName, cancellationToken)).ReturnsAsync(mockJobState);

        // Act
        var result = await endpoint.GetOrCreateJobState(jobName, repository.Object, cancellationToken);

        // Assert
        result.Should().BeOfType<Ok<ImportJobStateResponse>>();
        var okResult = result as Ok<ImportJobStateResponse>;
        okResult!.Value.Should().BeEquivalentTo(mockJobState.MapToResponse());
    }

    [Test, RecursiveMoqAutoData]
    public async Task GetOrCreateJobState_ReturnsBadRequest_WhenArgumentExceptionThrown(
        JobName jobName,
        [Frozen] Mock<IImportJobStateRepository> repository,
        [Greedy] JobsController endpoint,
        CancellationToken cancellationToken)
    {
        // Arrange
        var exception = new ArgumentException("Invalid job name provided");
        repository.Setup(r => r.GetOrCreateJobStateAsync(jobName, cancellationToken)).ThrowsAsync(exception);

        // Act
        var result = await endpoint.GetOrCreateJobState(jobName, repository.Object, cancellationToken);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
        var problemResult = result as ProblemHttpResult;
        problemResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Test, RecursiveMoqAutoData]
    public async Task GetOrCreateJobState_ReturnsInternalServerError_WhenExceptionThrown(
        JobName jobName,
        [Frozen] Mock<IImportJobStateRepository> repository,
        [Greedy] JobsController endpoint,
        CancellationToken cancellationToken)
    {
        // Arrange
        repository.Setup(r => r.GetOrCreateJobStateAsync(jobName, cancellationToken)).ThrowsAsync(new Exception());

        // Act
        var result = await endpoint.GetOrCreateJobState(jobName, repository.Object, cancellationToken);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
        var problemResult = result as ProblemHttpResult;
        problemResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Test, RecursiveMoqAutoData]
    public async Task GetOrCreateJobState_CallsRepositoryOnce_WithCorrectParameters(
        JobName jobName,
        ImportJobStateEntity mockJobState,
        [Frozen] Mock<IImportJobStateRepository> repository,
        [Greedy] JobsController endpoint,
        CancellationToken cancellationToken)
    {
        // Arrange
        repository.Setup(r => r.GetOrCreateJobStateAsync(jobName, cancellationToken)).ReturnsAsync(mockJobState);

        // Act
        await endpoint.GetOrCreateJobState(jobName, repository.Object, cancellationToken);

        // Assert
        repository.Verify(r => r.GetOrCreateJobStateAsync(jobName, cancellationToken), Times.Once);
    }
}