using Microsoft.EntityFrameworkCore;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Data.Repositories;

public interface IImportJobStateRepository
{
    Task<ImportJobStateEntity?> GetJobStateAsync(long accountId, JobName jobName, CancellationToken cancellationToken);

    /// <summary>
    /// Gets existing job state or creates a new one
    /// </summary>
    Task<ImportJobStateEntity> GetOrCreateJobStateAsync(long accountId, JobName jobName, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing job state
    /// </summary>
    Task<ImportJobStateEntity?> UpdateJobStateAsync(
        ImportJobStateEntity entity,
        CancellationToken cancellationToken);
}

public class ImportJobStateRepository(IFundingProjectionDataContext dataContext) : IImportJobStateRepository
{
    public async Task<ImportJobStateEntity?> GetJobStateAsync(long accountId, JobName jobName, CancellationToken cancellationToken)
    {
        return await dataContext.ImportJobStates.FirstOrDefaultAsync(j => j.AccountId == accountId && j.JobName == jobName, cancellationToken);
    }

    public async Task<ImportJobStateEntity> GetOrCreateJobStateAsync(long accountId, JobName jobName, CancellationToken cancellationToken)
    {
        var jobState = await GetJobStateAsync(accountId, jobName, cancellationToken);

        if (jobState != null)
        {
            return jobState;
        }

        jobState = new ImportJobStateEntity
        {
            AccountId = accountId,
            JobName = jobName,
            JobState = JobState.Pending,
            LastSuccessfulImportDate = DateTime.UtcNow.AddDays(-30),
            LastAttemptedDate = DateTime.UtcNow,
            LastAttemptSuccessful = false,
            TotalRecordsLastRun = 0,
            FailedRecordsLastRun = 0,
            UpdatedDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        };

        await dataContext.ImportJobStates.AddAsync(jobState, cancellationToken);
        await dataContext.SaveChangesAsync(cancellationToken);

        return jobState;
    }

    public async Task<ImportJobStateEntity?> UpdateJobStateAsync(
        ImportJobStateEntity entity,
        CancellationToken cancellationToken)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var existingJobState = await GetJobStateAsync(entity.AccountId, entity.JobName, cancellationToken);

        if (existingJobState == null)
            return null;

        // Update properties
        existingJobState.JobState = entity.JobState;
        if (entity.LastAttemptSuccessful)
            existingJobState.LastSuccessfulImportDate = entity.LastSuccessfulImportDate;
        existingJobState.LastAttemptedDate = entity.LastAttemptedDate;
        existingJobState.LastAttemptSuccessful = entity.LastAttemptSuccessful;
        existingJobState.TotalRecordsLastRun = entity.TotalRecordsLastRun;
        existingJobState.FailedRecordsLastRun = entity.FailedRecordsLastRun;
        existingJobState.UpdatedDate = DateTime.UtcNow;

        dataContext.ImportJobStates.Update(existingJobState);
        await dataContext.SaveChangesAsync(cancellationToken);

        return existingJobState;
    }
}