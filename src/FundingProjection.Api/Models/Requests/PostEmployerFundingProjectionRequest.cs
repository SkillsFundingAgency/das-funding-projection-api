using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.Models.Requests;

public class PostEmployerFundingProjectionRequest
{
    public DateOnly? StartPeriod { get; set; }
    public int Months { get; set; } = 6;
    public decimal CurrentLevyFunds { get; set; } = 0m;
    public List<LevyInMonthSummary> HistoricLevyIn { get; set; } = [];
    public List<CalendarPeriodAmount> LevyFundsInForExpiry { get; set; } = [];
    public List<CalendarPeriodAmount> PaymentFundsOutForExpiry { get; set; } = [];
    public List<CalendarPeriodAmount> LongTermExpiredFunds { get; set; } = [];
    public List<CalendarPeriodAmount> ShortTermExpiredFunds { get; set; } = [];
    public CollectionPeriodPayments? CurrentPayments { get; set; }
}

public sealed record CollectionPeriodPayments
{
    public bool PeriodEndCompleted { get; init; }
    public List<CollectionPeriodPaymentTotal> Payments { get; init; } = [];
}

public sealed record CollectionPeriodPaymentTotal
{
    public FundingSource FundingSource { get; init; }
    public TransactionType TransactionType { get; init; }
    public decimal Amount { get; init; }
}

public sealed record CalendarPeriodAmount
{
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal Amount { get; init; }
}