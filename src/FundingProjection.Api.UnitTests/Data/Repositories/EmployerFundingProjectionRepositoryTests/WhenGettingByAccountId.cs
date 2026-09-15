using AutoFixture.NUnit4;
using FluentAssertions;
using Moq;
using SFA.DAS.FundingProjection.Api.UnitTests.Data.DatabaseMock;
using SFA.DAS.FundingProjection.Data;
using SFA.DAS.FundingProjection.Data.Repositories;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Data.Repositories.EmployerFundingProjectionRepositoryTests;

[TestFixture]
internal class WhenGettingByAccountId
{
    [Test, RecursiveMoqAutoData]
    public async Task Then_The_EmployerFundingProjections_Are_Returned_By_AccountId(
        long employerAccountId,
        CancellationToken token,
        EmployerFundingProjectionEntity employerFundingProjectionEntity,
        [Frozen] Mock<IFundingProjectionDataContext> context,
        [Greedy] EmployerFundingProjectionRepository repository)
    {
        employerFundingProjectionEntity.EmployerAccountId = employerAccountId;
        context.Setup(x => x.EmployerFundingProjections)
            .ReturnsDbSet(new List<EmployerFundingProjectionEntity> { employerFundingProjectionEntity });
        
        var actual = await repository.GetByEmployerAccountIdAsync(employerAccountId, token);

        actual.Should().BeEquivalentTo(employerFundingProjectionEntity);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_The_ApplicationReviews_Not_Matched_Then_No_Results_Returned(
        long employerAccountId,
        CancellationToken token,
        List<EmployerFundingProjectionEntity> employerFundingProjectionEntities,
        [Frozen] Mock<IFundingProjectionDataContext> context,
        [Greedy] EmployerFundingProjectionRepository repository)
    {
        context.Setup(x => x.EmployerFundingProjections)
            .ReturnsDbSet(employerFundingProjectionEntities);

        var actual = await repository.GetByEmployerAccountIdAsync(employerAccountId, token);

        actual.Should().BeNull();
    }
}