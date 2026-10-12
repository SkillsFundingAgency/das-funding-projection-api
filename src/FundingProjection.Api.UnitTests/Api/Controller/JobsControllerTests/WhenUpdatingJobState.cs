using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using SFA.DAS.FundingProjection.Api.Controllers;
using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Data.Repositories;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Controller.JobsControllerTests;

[TestFixture]
internal class WhenUpdatingJobState
{
    [Test, RecursiveMoqAutoData]
    public async Task UpdateJobState_ReturnsOk_WhenJobStateUpdatedSuccessfully(Guid id,
        PutImportJobStateRequest request,
        [Frozen] Mock<IImportJobStateRepository> repository,
        [Greedy] JobsController endpoint, 
        CancellationToken cancellationToken)
    {
        // Arrange
        var mockJobState = new ImportJobStateEntity
        {
            Id = id,
            LastSuccessfulImportDate = request.LastSuccessfulImportDate,
            LastAttemptedDate = request.LastAttemptedDate,
            LastAttemptSuccessful = request.LastAttemptSuccessful,
            TotalRecordsLastRun = request.TotalRecordsLastRun,
            FailedRecordsLastRun = request.FailedRecordsLastRun
        };

        repository.Setup(r => r.UpdateJobStateAsync(mockJobState, CancellationToken.None)).ReturnsAsync(mockJobState);

        // Act
        var result = await endpoint.UpdateJobState(id, request, repository.Object, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Ok<ImportJobStateResponse>>();
    }

    [Test, RecursiveMoqAutoData]
    public async Task GetOrCrUpdateJobState_ReturnsInternalServerError_WhenExceptionThrown(
        Guid id,
        PutImportJobStateRequest request,
        [Frozen] Mock<IImportJobStateRepository> repository,
        [Greedy] JobsController endpoint,
        CancellationToken cancellationToken)
    {
        // Arrange
        repository.Setup(r => r.UpdateJobStateAsync(It.IsAny<ImportJobStateEntity>(), cancellationToken)).ThrowsAsync(new Exception());

        // Act
        var result = await endpoint.UpdateJobState(id, request, repository.Object, cancellationToken);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
        var problemResult = result as ProblemHttpResult;
        problemResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}