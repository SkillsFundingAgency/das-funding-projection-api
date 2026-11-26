using SFA.DAS.FundingProjection.Api.UnitTests.Data.DatabaseMock;
using SFA.DAS.FundingProjection.Data;
using SFA.DAS.FundingProjection.Data.Repositories;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Data.Repositories.ImportJobStateRepositoryTests;

[TestFixture]
internal class WhenGettingJobState
{
    [Test, RecursiveMoqAutoData]
    public async Task Then_The_Job_State_For_The_Account_And_Job_Is_Returned(
        ImportJobStateEntity existing,
        ImportJobStateEntity otherAccount,
        [Frozen] Mock<IFundingProjectionDataContext> context,
        [Greedy] ImportJobStateRepository repository)
    {
        existing.JobName = JobName.ImportLearnersCost;
        otherAccount.JobName = JobName.ImportLearnersCost;
        context.Setup(x => x.ImportJobStates).ReturnsDbSet(new List<ImportJobStateEntity> { otherAccount, existing });

        var actual = await repository.GetJobState(existing.AccountId, JobName.ImportLearnersCost, CancellationToken.None);

        actual.Should().BeSameAs(existing);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_Null_Is_Returned_And_Nothing_Is_Created_When_There_Is_No_Job_State(
        long accountId,
        [Frozen] Mock<IFundingProjectionDataContext> context,
        [Greedy] ImportJobStateRepository repository)
    {
        context.Setup(x => x.ImportJobStates).ReturnsDbSet(new List<ImportJobStateEntity>());

        var actual = await repository.GetJobState(accountId, JobName.ImportLearnersCost, CancellationToken.None);

        actual.Should().BeNull();
        context.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
