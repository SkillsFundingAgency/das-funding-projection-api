using SFA.DAS.FundingProjection.Api.UnitTests.Data.DatabaseMock;
using SFA.DAS.FundingProjection.Data;
using SFA.DAS.FundingProjection.Data.Repositories;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Data.Repositories.EmployerFundingProjectionRepositoryTests;

[TestFixture]
internal class WhenGettingApprenticeshipSummariesAsync
{
    // Returns empty list when no projections exist
    [Test, RecursiveMoqAutoData]
    public async Task Then_Empty_List_Is_Returned_When_No_Projections_Exist(
        long accountId,
        CancellationToken token,
        [Frozen] Mock<IFundingProjectionDataContext> context,
        [Greedy] EmployerFundingProjectionRepository repository)
    {
        context.Setup(x => x.ApprenticeshipPaymentSummaries)
            .ReturnsDbSet(new List<ApprenticeshipPaymentSummaryEntity>());

        var actual = await repository.GetApprenticeshipSummariesAsync(accountId, token);

        actual.Should().BeEmpty();
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_The_ApprenticeshipPaymentSummaryEntity_Not_Matched_Then_No_Results_Returned(
        long accountId,
        CancellationToken token,
        List<ApprenticeshipPaymentSummaryEntity> apprenticeshipPaymentSummaryEntities,
        [Frozen] Mock<IFundingProjectionDataContext> context,
        [Greedy] EmployerFundingProjectionRepository repository)
    {
        context.Setup(x => x.ApprenticeshipPaymentSummaries)
            .ReturnsDbSet(apprenticeshipPaymentSummaryEntities);

        var actual = await repository.GetApprenticeshipSummariesAsync(accountId, token);

        actual.Should().BeEmpty();
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_The_ApprenticeshipPaymentSummaryEntity_Matched_Then_Results_Returned(
        long accountId,
        CancellationToken token,
        List<ApprenticeshipPaymentSummaryEntity> apprenticeshipPaymentSummaryEntities,
        [Frozen] Mock<IFundingProjectionDataContext> context,
        [Greedy] EmployerFundingProjectionRepository repository)
    {
        foreach (var entity in apprenticeshipPaymentSummaryEntities)
        {
            entity.AccountId = accountId;
        }
        context.Setup(x => x.ApprenticeshipPaymentSummaries)
            .ReturnsDbSet(apprenticeshipPaymentSummaryEntities);
        var actual = await repository.GetApprenticeshipSummariesAsync(accountId, token);
        actual.Should().BeEquivalentTo(apprenticeshipPaymentSummaryEntities);
    }
}