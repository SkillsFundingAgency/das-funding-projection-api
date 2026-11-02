using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.Projection;

public interface IApprenticeshipOrderingStrategy
{
    IOrderedEnumerable<ApprenticeshipPaymentSummaryEntity> Execute(List<ApprenticeshipPaymentSummaryEntity> apprenticeships);
}

public class ApprenticeshipOrderingStrategy: IApprenticeshipOrderingStrategy
{
    public IOrderedEnumerable<ApprenticeshipPaymentSummaryEntity> Execute(List<ApprenticeshipPaymentSummaryEntity> apprenticeships)
    {
        return apprenticeships
            .OrderByDescending(x => x.IsTransfer)
            .ThenBy(LearningTypePriority)
            .ThenBy(ProviderOrderOrFirstApprovalPriority)
            .ThenBy(x => x.ApprovalDate)
            .ThenBy(x => x.Uln);
    }
    private static long ProviderOrderOrFirstApprovalPriority(ApprenticeshipPaymentSummaryEntity entity)
    {
        return entity.ProviderOrder switch
        {
            null => entity.FirstApprovalDate is not null
                ? entity.FirstApprovalDate.Value.ToDateTime(TimeOnly.MinValue).Ticks
                : entity.ApprovalDate.Ticks,
            _ => entity.ProviderOrder.Value
        };
    }
    private static int LearningTypePriority(ApprenticeshipPaymentSummaryEntity entity)
    {
        return entity.LearningType switch
        {
            LearningType.ApprenticeshipUnit => 1,
            _ => 0
        };
    }
}