using Microsoft.EntityFrameworkCore;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Data.Repositories;

public interface IImportJobStateRepository
{
    /// <summary>
    /// Gets existing job state or creates a new one
    /// </summary>
    Task<ImportJobStateEntity> GetOrCreateJobStateAsync(JobName jobName, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing job state
    /// </summary>
    Task<ImportJobStateEntity> UpdateJobStateAsync(
        ImportJobStateEntity entity,
        CancellationToken cancellationToken);
}

public class ImportJobStateRepository(IFundingProjectionDataContext dataContext) : IImportJobStateRepository
{
    public async Task<ImportJobStateEntity> GetOrCreateJobStateAsync(JobName jobName, CancellationToken cancellationToken)
    {
        var jobState = await dataContext.ImportJobStates
            .FirstOrDefaultAsync(j => j.JobName == jobName, cancellationToken);

        if (jobState != null)
        {
            return jobState;
        }

        jobState = new ImportJobStateEntity
        {
            JobName = jobName,
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

    public async Task<ImportJobStateEntity> UpdateJobStateAsync(
        ImportJobStateEntity entity,
        CancellationToken cancellationToken)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var existingJobState = await dataContext.ImportJobStates
            .FirstOrDefaultAsync(j => j.Id == entity.Id, cancellationToken);

        if (existingJobState == null)
            throw new InvalidOperationException($"Job state with ID {entity.Id} not found");

        // Update properties
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