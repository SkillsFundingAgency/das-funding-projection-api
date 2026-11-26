using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using SFA.DAS.FundingProjection.Api.Controllers;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Data.Repositories;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Controller.JobsControllerTests;

[TestFixture]
internal class WhenGettingJobState
{
    [Test, RecursiveMoqAutoData]
    public async Task GetJobState_ReturnsOk_WithTheJobState_WhenItExists(
        long accountId,
        ImportJobStateEntity jobState,
        [Frozen] Mock<IImportJobStateRepository> repository,
        [Greedy] JobsController endpoint,
        CancellationToken cancellationToken)
    {
        // Arrange
        repository.Setup(r => r.GetJobState(accountId, JobName.ImportLearnersCost, cancellationToken)).ReturnsAsync(jobState);

        // Act
        var result = await endpoint.GetJobState(accountId, JobName.ImportLearnersCost, repository.Object, cancellationToken);

        // Assert
        var okResult = result.Should().BeOfType<Ok<ImportJobStateResponse>>().Subject;
        okResult.Value!.Id.Should().Be(jobState.Id);
        okResult.Value.LastSuccessfulImportDate.Should().Be(jobState.LastSuccessfulImportDate);
    }

    [Test, RecursiveMoqAutoData]
    public async Task GetJobState_ReturnsNotFound_WhenThereIsNoJobState(
        long accountId,
        [Frozen] Mock<IImportJobStateRepository> repository,
        [Greedy] JobsController endpoint,
        CancellationToken cancellationToken)
    {
        // Arrange
        repository.Setup(r => r.GetJobState(accountId, JobName.ImportLearnersCost, cancellationToken))
            .ReturnsAsync((ImportJobStateEntity?)null);

        // Act
        var result = await endpoint.GetJobState(accountId, JobName.ImportLearnersCost, repository.Object, cancellationToken);

        // Assert
        result.Should().BeOfType<NotFound>();
    }

    [Test, RecursiveMoqAutoData]
    public async Task GetJobState_DoesNotCreateAJobState(
        long accountId,
        [Frozen] Mock<IImportJobStateRepository> repository,
        [Greedy] JobsController endpoint,
        CancellationToken cancellationToken)
    {
        // Act
        await endpoint.GetJobState(accountId, JobName.ImportLearnersCost, repository.Object, cancellationToken);

        // Assert
        repository.Verify(r => r.GetOrCreateJobState(It.IsAny<long>(), It.IsAny<JobName>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, RecursiveMoqAutoData]
    public async Task GetJobState_ReturnsInternalServerError_WhenExceptionThrown(
        long accountId,
        [Frozen] Mock<IImportJobStateRepository> repository,
        [Greedy] JobsController endpoint,
        CancellationToken cancellationToken)
    {
        // Arrange
        repository.Setup(r => r.GetJobState(accountId, JobName.ImportLearnersCost, cancellationToken)).ThrowsAsync(new Exception());

        // Act
        var result = await endpoint.GetJobState(accountId, JobName.ImportLearnersCost, repository.Object, cancellationToken);

        // Assert
        var problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}
