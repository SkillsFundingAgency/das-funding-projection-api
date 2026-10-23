namespace SFA.DAS.FundingProjection.Api.Projection;

public class ScheduledPayment
{
    public DateOnly SourcePeriod { get; set; }
    public DateOnly PaymentPeriod { get; set; }
    public decimal ScheduledAmount { get; set; }
    public decimal FinalPayment { get; set; }
}