using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Api.Projection;

internal class CopaymentRule
{
    public Func<ApprenticeshipPaymentSummaryEntity, bool> CanRun { get; private init; } = _ => false;
    public Func<decimal, decimal> CalculateCopayment { get; private init; } = _ => 0;
        
    public static CopaymentRule Create(Func<ApprenticeshipPaymentSummaryEntity, bool> test, Func<decimal, decimal> calc) 
        => new() { CanRun = test, CalculateCopayment = calc };
}