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
        var defaultProviderOrdering = apprenticeships
            .Where(x => x.ProviderOrder is null)
            .GroupBy(x => x.ProviderId)
            .Select(x => x.OrderBy(c => c.ApprovalDate).First())
            .OrderBy(x => x.ApprovalDate)
            .Select(x => x.ProviderId)
            .ToList();

        return apprenticeships
            .OrderByDescending(x => x.IsTransfer)
            .ThenBy(LearningTypePriority)
            .ThenBy(NullLastPriority)
            .ThenBy(x => defaultProviderOrdering.IndexOf(x.ProviderId))
            .ThenBy(x => x.ApprovalDate)
            .ThenBy(x => x.Uln);
    }

    private static int NullLastPriority(ApprenticeshipPaymentSummaryEntity entity)
    {
        return entity.ProviderOrder switch
        {
            null => int.MaxValue,
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