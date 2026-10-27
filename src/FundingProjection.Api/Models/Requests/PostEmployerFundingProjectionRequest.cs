using SFA.DAS.FundingProjection.Api.Projection;

namespace SFA.DAS.FundingProjection.Api.Models.Requests;

public class PostEmployerFundingProjectionRequest
{
    public int Months { get; init; } = 6;
    public decimal CurrentLevyFunds { get; init; } = 0m;
    public List<LevyInMonthSummary> HistoricLevyIn { get; init; } = [];
    public List<CalendarPeriodAmount> LevyFundsInForExpiry { get; init; } = [];
    public List<CalendarPeriodAmount> PaymentFundsOutForExpiry { get; set; } = [];
    public List<CalendarPeriodAmount> LongTermExpiredFunds { get; set; } = [];
    public List<CalendarPeriodAmount> ShortTermExpiredFunds { get; set; } = [];
}

public sealed record CalendarPeriodAmount
{
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal Amount { get; init; }
}