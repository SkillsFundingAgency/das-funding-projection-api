using Microsoft.AspNetCore.Http.HttpResults;
using SFA.DAS.FundingProjection.Api.Controllers;
using SFA.DAS.FundingProjection.Api.Core;
using SFA.DAS.FundingProjection.Api.Models.Mappers;
using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Data.Models;
using SFA.DAS.FundingProjection.Data.Repositories;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Controller.ApprenticePaymentSummary;

[TestFixture]
internal class WhenPostingApprenticeshipPaymentSummary
{
    [Test, RecursiveMoqAutoData]
    public async Task Put_ReturnsCreated_WhenEntityIsNew(long accountId,
    long apprenticeshipId,
    PostApprenticeshipPaymentSummaryRequest request,
    ApprenticeshipPaymentSummaryEntity entity,
    [Frozen] Mock<IApprenticeshipPaymentSummariesRepository> repository,
    [Greedy] ApprenticeshipPaymentSummaryController endpoint)
    {
        // Arrange
        var upsertResult = UpsertResult.Create(entity, true);

        repository
            .Setup(r => r.UpsertOneAsync(
                It.IsAny<ApprenticeshipPaymentSummaryEntity>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(upsertResult);

        // Act
        var result = await endpoint.Put(
            repository.Object, accountId, apprenticeshipId, request, CancellationToken.None);

        // Assert
        var created = result.Should().BeOfType<Created<ApprenticeshipPaymentSummaryResponse>>().Subject;
        created.Location.Should().Be($"/{RouteNames.ApprenticeshipPaymentSummary}/{apprenticeshipId}");
        created.Value.Should().BeEquivalentTo(entity.ToResponse());
    }

    [Test, RecursiveMoqAutoData]
    public async Task Put_ReturnsOk_WhenEntityAlreadyExists(long accountId,
        long apprenticeshipId,
        PostApprenticeshipPaymentSummaryRequest request,
        ApprenticeshipPaymentSummaryEntity entity,
        [Frozen] Mock<IApprenticeshipPaymentSummariesRepository> repository,
        [Greedy] ApprenticeshipPaymentSummaryController endpoint)
    {
        // Arrange
        var upsertResult = UpsertResult.Create(entity, false); ;

        repository
            .Setup(r => r.UpsertOneAsync(
                It.IsAny<ApprenticeshipPaymentSummaryEntity>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(upsertResult);

        // Act
        var result = await endpoint.Put(
            repository.Object, accountId, apprenticeshipId, request, CancellationToken.None);

        // Assert
        var ok = result.Should().BeOfType<Ok<ApprenticeshipPaymentSummaryResponse>>().Subject;
        ok.Value.Should().BeEquivalentTo(entity.ToResponse());
    }

    [Test, RecursiveMoqAutoData]
    public async Task Put_MapsRouteParametersOntoEntity(
        long accountId,
        long apprenticeshipId,
        PostApprenticeshipPaymentSummaryRequest request,
        ApprenticeshipPaymentSummaryEntity entity,
        [Frozen] Mock<IApprenticeshipPaymentSummariesRepository> repository,
        [Greedy] ApprenticeshipPaymentSummaryController endpoint)
    {
        // Arrange
        repository
            .Setup(r => r.UpsertOneAsync(
                It.IsAny<ApprenticeshipPaymentSummaryEntity>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(UpsertResult.Create(entity, true));

        // Act
        await endpoint.Put(
            repository.Object, accountId, apprenticeshipId, request, CancellationToken.None);

        // Assert — verify accountId and apprenticeshipId were threaded through to the entity
        repository.Verify(r => r.UpsertOneAsync(
            It.Is<ApprenticeshipPaymentSummaryEntity>(e =>
                e.AccountId == accountId &&
                e.ApprenticeshipId == apprenticeshipId),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}