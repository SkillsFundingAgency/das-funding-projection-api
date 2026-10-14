using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.Models.Requests;

public sealed record PutImportJobStateRequest
{
    public long AccountId { get; set; }
    public JobName JobName { get; set; }
    public DateTime LastSuccessfulImportDate { get; set; }
    public DateTime LastAttemptedDate { get; set; }
    public bool LastAttemptSuccessful { get; set; }
    public int TotalRecordsLastRun { get; set; }
    public int FailedRecordsLastRun { get; set; }
}