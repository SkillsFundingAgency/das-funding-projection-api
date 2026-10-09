namespace SFA.DAS.FundingProjection.Api.Models.Responses;

public sealed record MonthlyFundingBreakdown
{
    public long EmployerAccountId { get; init; }
    public int Month { get; init; }
    public int Year { get; init; }
    public decimal ClosingBalance { get; set; }
    public decimal CommittedLearnerCost { get; init; }
    public decimal CommittedLearnerFinalPaymentCost { get; init; }
    public decimal CommittedTransferOut { get; init; }
    public decimal LevyIn { get; init; }
}