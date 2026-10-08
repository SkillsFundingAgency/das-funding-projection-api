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

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Controller.LearnerControllerTests;

[TestFixture]
internal class WhenGettingLearnersByStatus
{
    [Test, RecursiveMoqAutoData]
    public async Task GetMany_ReturnsOk_WhenLearnersExistForStatus(
        ImportStatus importStatus,
        List<CommittedLearnerEntity> mockLearners,
        [Frozen] Mock<ICommittedLearnerRepository> repository,
        [Greedy] LearnerController endpoint,
        CancellationToken token)
    {
        // Arrange
        repository.Setup(r => r.GetAllImportStatus(importStatus, token)).ReturnsAsync(mockLearners);

        // Act
        var result = await endpoint.GetMany(importStatus, repository.Object, token);

        // Assert
        result.Should().BeOfType<Ok<IEnumerable<GetCommittedLearnerResponse>>>();
        var okResult = result as Ok<IEnumerable<GetCommittedLearnerResponse>>;
        okResult!.Value.Should().BeEquivalentTo(mockLearners.Select(lr => lr.ToResponse()));
    }

    [Test, RecursiveMoqAutoData]
    public async Task GetMany_ReturnsOkWithEmptyList_WhenNoLearnersExistForStatus(
        ImportStatus importStatus,
        [Frozen] Mock<ICommittedLearnerRepository> repository,
        [Greedy] LearnerController endpoint,
        CancellationToken token)
    {
        // Arrange
        repository.Setup(r => r.GetAllImportStatus(importStatus, token)).ReturnsAsync(new List<CommittedLearnerEntity>());

        // Act
        var result = await endpoint.GetMany(importStatus, repository.Object, token);

        // Assert
        result.Should().BeOfType<Ok<IEnumerable<GetCommittedLearnerResponse>>>();
        var okResult = result as Ok<IEnumerable<GetCommittedLearnerResponse>>;
        okResult!.Value.Should().BeEmpty();
    }

    [Test, RecursiveMoqAutoData]
    public async Task GetMany_ReturnsInternalServerError_WhenExceptionThrown(
        ImportStatus importStatus,
        [Frozen] Mock<ICommittedLearnerRepository> repository,
        [Greedy] LearnerController endpoint,
        CancellationToken token)
    {
        // Arrange
        repository.Setup(r => r.GetAllImportStatus(importStatus, token)).ThrowsAsync(new Exception());

        // Act
        var result = await endpoint.GetMany(importStatus, repository.Object, token);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
        var problemResult = result as ProblemHttpResult;
        problemResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}
