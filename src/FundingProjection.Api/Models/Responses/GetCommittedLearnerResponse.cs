using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.Models.Responses;

public sealed record GetCommittedLearnerResponse
{
    public Guid Id { get; set; }
    public long ApprenticeshipId { get; set; }
    public long? CommitmentId { get; set; }
    public decimal Cost { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public DateTime ImportedOn { get; set; }
    public required PaymentStatus PaymentStatus { get; set; }
    public required ImportStatus ImportStatus { get; set; }
}