namespace SFA.DAS.FundingProjection.Api.Models.Responses;

public sealed record MonthlyFundingBreakdown
{
    public long EmployerAccountId { get; init; }
    public int Month { get; init; }
    public int Year { get; init; }
    public decimal ClosingBalance { get; set; }
    public required LevyInSummary LevyInSummary { get; init; }
    public required LevyOutSummary LevyOutSummary { get; init; }
}

public sealed record LevyOutSummary
{
    public decimal Total => CommittedLearnerCost + CommittedLearnerFinalPaymentCost + CommittedTransferOut + CommittedTransferOutFinalPaymentCost + ExpiredLevy;
    public decimal CommittedLearnerCost { get; init; } = 0m;
    public decimal CommittedLearnerFinalPaymentCost { get; init; } = 0m;
    public decimal CommittedTransferOut { get; init; } = 0m;
    public decimal CommittedTransferOutFinalPaymentCost { get; init; } = 0m;
    public decimal ExpiredLevy { get; init; } = 0m;
}

public sealed record LevyInSummary
{
    public decimal Total => LevyIn;
    public decimal LevyIn { get; init; } = 0m;
}