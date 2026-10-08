using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Api.Models.Mappers;

public static class ImportJobStateExtensions
{
    public static ImportJobStateResponse MapToResponse(this ImportJobStateEntity entity)
    {
        return new ImportJobStateResponse
        {
            Id = entity.Id,
            JobName = entity.JobName,
            LastSuccessfulImportDate = entity.LastSuccessfulImportDate,
            LastAttemptedDate = entity.LastAttemptedDate,
            LastAttemptSuccessful = entity.LastAttemptSuccessful,
            TotalRecordsLastRun = entity.TotalRecordsLastRun,
            FailedRecordsLastRun = entity.FailedRecordsLastRun,
            UpdatedDate = entity.UpdatedDate,
            CreatedDate = entity.CreatedDate
        };
    }
}