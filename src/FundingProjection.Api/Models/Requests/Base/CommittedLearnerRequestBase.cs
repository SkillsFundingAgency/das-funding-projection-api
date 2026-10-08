using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.Models.Requests.Base;

public abstract record CommittedLearnerRequestBase
{
    public long ApprenticeshipId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public required PaymentStatus PaymentStatus { get; set; }
}