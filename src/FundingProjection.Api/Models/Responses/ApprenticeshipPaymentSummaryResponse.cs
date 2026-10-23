using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.Models.Responses;

public class ApprenticeshipPaymentSummaryResponse
{
    public long ApprenticeshipId { get; set; }
    public long AccountId { get; set; }
    public bool IsTransfer { get; set; }
    public long Uln { get; set; }
    public int AgeAtStart { get; set; }
    public ApprenticeshipStatus Status { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalPaid { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateOnly? LastPaymentDate { get; set; }
    public decimal? LastPaymentAmount { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
}