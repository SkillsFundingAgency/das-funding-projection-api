using SFA.DAS.FundingProjection.Api.UnitTests.Data.DatabaseMock;
using SFA.DAS.FundingProjection.Data;
using SFA.DAS.FundingProjection.Data.Repositories;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Data.Repositories.ImportJobStateRepositoryTests;

[TestFixture]
internal class WhenUpdatingJobStateAsync
{
    [Test, RecursiveMoqAutoData]
    public async Task Then_The_Job_State_And_Last_Run_Are_Updated_When_Successful(
        ImportJobStateEntity existing,
        DateTime lastSuccessfulImportDate,
        [Frozen] Mock<IFundingProjectionDataContext> context,
        [Greedy] ImportJobStateRepository repository)
    {
        existing.JobState = JobState.Pending;
        context.Setup(x => x.ImportJobStates).ReturnsDbSet(new List<ImportJobStateEntity> { existing });

        var actual = await repository.UpdateJobStateAsync(CreateUpdate(existing, JobState.Completed, true, lastSuccessfulImportDate), CancellationToken.None);

        actual.JobState.Should().Be(JobState.Completed);
        actual.LastAttemptSuccessful.Should().BeTrue();
        actual.LastSuccessfulImportDate.Should().Be(lastSuccessfulImportDate);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_The_Last_Successful_Import_Date_Is_Kept_When_Failed(
        ImportJobStateEntity existing,
        [Frozen] Mock<IFundingProjectionDataContext> context,
        [Greedy] ImportJobStateRepository repository)
    {
        var previousSuccessfulImportDate = existing.LastSuccessfulImportDate;
        existing.JobState = JobState.Pending;
        context.Setup(x => x.ImportJobStates).ReturnsDbSet(new List<ImportJobStateEntity> { existing });

        var actual = await repository.UpdateJobStateAsync(CreateUpdate(existing, JobState.Failed, false, DateTime.MinValue), CancellationToken.None);

        actual.JobState.Should().Be(JobState.Failed);
        actual.LastAttemptSuccessful.Should().BeFalse();
        actual.LastSuccessfulImportDate.Should().Be(previousSuccessfulImportDate);
    }

    private static ImportJobStateEntity CreateUpdate(ImportJobStateEntity existing, JobState jobState, bool successful, DateTime lastSuccessfulImportDate) =>
        new()
        {
            Id = existing.Id,
            AccountId = existing.AccountId,
            JobName = existing.JobName,
            JobState = jobState,
            LastSuccessfulImportDate = lastSuccessfulImportDate,
            LastAttemptedDate = DateTime.UtcNow,
            LastAttemptSuccessful = successful,
            TotalRecordsLastRun = 10,
            FailedRecordsLastRun = successful ? 0 : 10
        };
}
