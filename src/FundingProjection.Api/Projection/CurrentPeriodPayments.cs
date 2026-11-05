using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.Projection;

public sealed record CurrentPeriodPayments
{
    public decimal LearnerCost { get; init; }
    public decimal LearnerFinalPaymentCost { get; init; }
    public decimal TransferOut { get; init; }
    public decimal TransferOutFinalPaymentCost { get; init; }
    public decimal CoInvestment { get; init; }

    public static CurrentPeriodPayments? From(CollectionPeriodPayments? payments)
    {
        if (payments is not { PeriodEndCompleted: true })
        {
            return null;
        }

        return new CurrentPeriodPayments
        {
            LearnerCost = Sum(payments, FundingSource.Levy, completion: false),
            LearnerFinalPaymentCost = Sum(payments, FundingSource.Levy, completion: true),
            TransferOut = Sum(payments, FundingSource.LevyTransfer, completion: false),
            TransferOutFinalPaymentCost = Sum(payments, FundingSource.LevyTransfer, completion: true),
            CoInvestment = payments.Payments.Where(p => p.FundingSource == FundingSource.CoInvestedEmployer).Sum(p => p.Amount)
        };
    }

    private static decimal Sum(CollectionPeriodPayments payments, FundingSource fundingSource, bool completion) =>
        payments.Payments
            .Where(p => p.FundingSource == fundingSource && (p.TransactionType == TransactionType.Completion) == completion)
            .Sum(p => p.Amount);
}
