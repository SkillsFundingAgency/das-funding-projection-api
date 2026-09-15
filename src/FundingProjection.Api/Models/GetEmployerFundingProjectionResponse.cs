namespace SFA.DAS.FundingProjection.Api.Models;

public record GetEmployerFundingProjectionResponse
{
    public long EmployerAccountId { get; init; }
    public decimal CommittedLearnerCostTotal { get; init; }
    public decimal CommittedTransferOutTotal { get; init; }
    public DateTime LastRecalculatedDate { get; init; }
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
}
