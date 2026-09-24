using AutoFixture.NUnit4;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using SFA.DAS.FundingProjection.Api.Controllers;
using SFA.DAS.FundingProjection.Api.Models.Mappers;
using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Data.Models;
using SFA.DAS.FundingProjection.Data.Repositories;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Controller.LearnerControllerTests;

[TestFixture]
internal class WhenPuttingOne
{
    [Test, RecursiveMoqAutoData]
    public async Task PutOne_ReturnsCreated_WhenLearnerIsNewlyCreated(
        Guid id,
        long accountId,
        PutCommittedLearnerRequest request,
        UpsertResult<CommittedLearnerEntity> mockResult,
        [Frozen] Mock<ICommittedLearnerRepository> repository,
        [Frozen] Mock<IValidator<PutCommittedLearnerRequest>> validator,
        [Greedy] LearnerController endpoint,
        CancellationToken token)
    {
        // Arrange
        mockResult = mockResult with { Created = true };
        validator.Setup(v => v.ValidateAsync(request, token)).ReturnsAsync(new ValidationResult());
        repository.Setup(r => r.UpsertOneAsync(It.IsAny<CommittedLearnerEntity>(), token)).ReturnsAsync(mockResult);

        // Act
        var result = await endpoint.PutOne(id, accountId, repository.Object, validator.Object, request, token);

        // Assert
        result.Should().BeOfType<Created<GetCommittedLearnerResponse>>();
        var createdResult = result as Created<GetCommittedLearnerResponse>;
        createdResult!.Value.Should().BeEquivalentTo(mockResult.Entity.ToResponse());
        createdResult.Location.Should().Be($"{mockResult.Entity.Id}");
    }

    [Test, RecursiveMoqAutoData]
    public async Task PutOne_ReturnsOk_WhenLearnerAlreadyExists(
        Guid id,
        long accountId,
        PutCommittedLearnerRequest request,
        UpsertResult<CommittedLearnerEntity> mockResult,
        [Frozen] Mock<ICommittedLearnerRepository> repository,
        [Frozen] Mock<IValidator<PutCommittedLearnerRequest>> validator,
        [Greedy] LearnerController endpoint,
        CancellationToken token)
    {
        // Arrange
        mockResult = mockResult with { Created = false };
        validator.Setup(v => v.ValidateAsync(request, token)).ReturnsAsync(new ValidationResult());
        repository.Setup(r => r.UpsertOneAsync(It.IsAny<CommittedLearnerEntity>(), token)).ReturnsAsync(mockResult);

        // Act
        var result = await endpoint.PutOne(id, accountId, repository.Object, validator.Object, request, token);

        // Assert
        result.Should().BeOfType<Ok<GetCommittedLearnerResponse>>();
        var okResult = result as Ok<GetCommittedLearnerResponse>;
        okResult!.Value.Should().BeEquivalentTo(mockResult.Entity.ToResponse());
    }

    [Test, RecursiveMoqAutoData]
    public async Task PutOne_ReturnsValidationProblem_WhenRequestIsInvalid(
        Guid id,
        long accountId,
        PutCommittedLearnerRequest request,
        [Frozen] Mock<ICommittedLearnerRepository> repository,
        [Frozen] Mock<IValidator<PutCommittedLearnerRequest>> validator,
        [Greedy] LearnerController endpoint,
        CancellationToken token)
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("FieldName", "FieldName is required")
        };
        validator.Setup(v => v.ValidateAsync(request, token)).ReturnsAsync(new ValidationResult(failures));

        // Act
        var result = await endpoint.PutOne(id, accountId, repository.Object, validator.Object, request, token);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
    }

    [Test, RecursiveMoqAutoData]
    public async Task PutOne_DoesNotCallRepository_WhenRequestIsInvalid(
        Guid id,
        long accountId,
        PutCommittedLearnerRequest request,
        [Frozen] Mock<ICommittedLearnerRepository> repository,
        [Frozen] Mock<IValidator<PutCommittedLearnerRequest>> validator,
        [Greedy] LearnerController endpoint,
        CancellationToken token)
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("FieldName", "FieldName is required")
        };
        validator.Setup(v => v.ValidateAsync(request, token)).ReturnsAsync(new ValidationResult(failures));

        // Act
        await endpoint.PutOne(id, accountId, repository.Object, validator.Object, request, token);

        // Assert
        repository.Verify(r => r.UpsertOneAsync(It.IsAny<CommittedLearnerEntity>(), token), Times.Never);
    }

    [Test, RecursiveMoqAutoData]
    public async Task PutOne_ReturnsInternalServerError_WhenExceptionThrown(
        Guid id,
        long accountId,
        PutCommittedLearnerRequest request,
        [Frozen] Mock<ICommittedLearnerRepository> repository,
        [Frozen] Mock<IValidator<PutCommittedLearnerRequest>> validator,
        [Greedy] LearnerController endpoint,
        CancellationToken token)
    {
        // Arrange
        validator.Setup(v => v.ValidateAsync(request, token)).ReturnsAsync(new ValidationResult());
        repository.Setup(r => r.UpsertOneAsync(It.IsAny<CommittedLearnerEntity>(), token)).ThrowsAsync(new Exception());

        // Act
        var result = await endpoint.PutOne(id, accountId, repository.Object, validator.Object, request, token);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
        var problemResult = result as ProblemHttpResult;
        problemResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}