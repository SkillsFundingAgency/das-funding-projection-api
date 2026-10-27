using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Api.Projection;

public class TimelineData
{
    public required long AccountId { get; init; }
    public required int Months { get; init; }
    public required DateOnly StartingPeriod { get; init; }
    public required decimal OpeningBalance { get; init; }

    public List<ApprenticeshipPaymentSummaryEntity> ApprenticeshipPaymentSummaries { get; init; } = [];
    public List<LevyInMonthSummary> HistoricLevyIn { get; init; } = [];
    public List<CalendarPeriodAmount> LevyFundsInForExpiry { get; init; } = [];
    public List<CalendarPeriodAmount> PaymentFundsOutForExpiry { get; init; } = [];
    public List<CalendarPeriodAmount> ShortTermExpiredFunds { get; init; } = [];
    public List<CalendarPeriodAmount> LongTermExpiredFunds { get; init; } = [];
}