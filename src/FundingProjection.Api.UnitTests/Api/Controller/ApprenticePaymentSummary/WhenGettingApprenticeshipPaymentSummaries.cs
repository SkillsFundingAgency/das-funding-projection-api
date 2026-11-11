using Microsoft.AspNetCore.Http.HttpResults;
using SFA.DAS.FundingProjection.Api.Controllers;
using SFA.DAS.FundingProjection.Api.Models.Mappers;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Data.Repositories;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Controller.ApprenticePaymentSummary;

[TestFixture]
internal class WhenGettingApprenticeshipPaymentSummaries
{
    [Test, RecursiveMoqAutoData]
    public async Task GetAll_ReturnsTheSummariesForTheAccount(long accountId,
        List<ApprenticeshipPaymentSummaryEntity> entities,
        [Frozen] Mock<IApprenticeshipPaymentSummariesRepository> repository,
        [Greedy] ApprenticeshipPaymentSummaryController endpoint)
    {
        // Arrange
        repository
            .Setup(r => r.GetApprenticeshipPaymentSummariesAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        // Act
        var result = await endpoint.GetAll(repository.Object, accountId, CancellationToken.None);

        // Assert
        var ok = result.Should().BeOfType<Ok<List<ApprenticeshipPaymentSummaryResponse>>>().Subject;
        ok.Value.Should().BeEquivalentTo(entities.Select(e => e.ToResponse()));
    }

    [Test, RecursiveMoqAutoData]
    public async Task GetAll_ReturnsAnEmptyList_WhenTheAccountHasNoSummaries(long accountId,
        [Frozen] Mock<IApprenticeshipPaymentSummariesRepository> repository,
        [Greedy] ApprenticeshipPaymentSummaryController endpoint)
    {
        // Arrange
        repository
            .Setup(r => r.GetApprenticeshipPaymentSummariesAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await endpoint.GetAll(repository.Object, accountId, CancellationToken.None);

        // Assert
        var ok = result.Should().BeOfType<Ok<List<ApprenticeshipPaymentSummaryResponse>>>().Subject;
        ok.Value.Should().BeEmpty();
    }
}
