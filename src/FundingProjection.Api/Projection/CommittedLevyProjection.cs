namespace SFA.DAS.FundingProjection.Api.Projection;

public class CommittedLevyProjection
{
    public DateOnly Period { get; set; }
    public decimal TotalMonthlyPayments { get; set; }
    public decimal TotalFinalPayments { get; set; }
}