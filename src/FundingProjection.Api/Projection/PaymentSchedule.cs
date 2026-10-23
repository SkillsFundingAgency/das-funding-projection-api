namespace SFA.DAS.FundingProjection.Api.Projection;

public class PaymentSchedule
{
    public long ApprenticeshipId { get; set; }
    public List<ScheduledPayment> ScheduledPayments { get; init; } = [];
}