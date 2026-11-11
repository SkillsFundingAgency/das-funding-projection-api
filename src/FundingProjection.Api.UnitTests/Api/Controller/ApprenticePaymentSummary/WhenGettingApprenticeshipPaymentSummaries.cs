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
        var result = await endpoint.GetAll(repository.Object, accountId, null, CancellationToken.None);

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
        var result = await endpoint.GetAll(repository.Object, accountId, null, CancellationToken.None);

        // Assert
        var ok = result.Should().BeOfType<Ok<List<ApprenticeshipPaymentSummaryResponse>>>().Subject;
        ok.Value.Should().BeEmpty();
    }

    [Test, RecursiveMoqAutoData]
    public async Task GetAll_ReturnsTheSummariesAwaitingPayment_WhenAPaymentPeriodIsGiven(long accountId,
        List<ApprenticeshipPaymentSummaryEntity> entities,
        [Frozen] Mock<IApprenticeshipPaymentSummariesRepository> repository,
        [Greedy] ApprenticeshipPaymentSummaryController endpoint)
    {
        // Arrange
        repository
            .Setup(r => r.GetApprenticeshipPaymentSummariesAwaitingPaymentAsync(accountId, new DateOnly(2025, 11, 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        // Act
        // Any day in the month is treated as that month's payment period
        var result = await endpoint.GetAll(repository.Object, accountId, new DateOnly(2025, 11, 15), CancellationToken.None);

        // Assert
        var ok = result.Should().BeOfType<Ok<List<ApprenticeshipPaymentSummaryResponse>>>().Subject;
        ok.Value.Should().BeEquivalentTo(entities.Select(e => e.ToResponse()));
        repository.Verify(r => r.GetApprenticeshipPaymentSummariesAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
