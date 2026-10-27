using SFA.DAS.EmployerFinance.Types.Models;
using SFA.DAS.FundingProjection.Api.Models.Mappers;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.Projection;

public class TimelineGenerator(IExpiredFunds expiredFunds)
{
    private static bool ApprenticeshipsHaveRemainingCost(ApprenticeshipPaymentSummaryEntity entity) 
        => entity.Status is ApprenticeshipStatus.Live
            or ApprenticeshipStatus.WaitingToStart
            or ApprenticeshipStatus.Paused;

    private static bool ApprenticeshipCanBeProjected(ApprenticeshipPaymentSummaryEntity entity) =>
        entity.Status is ApprenticeshipStatus.Live
            or ApprenticeshipStatus.WaitingToStart
            or ApprenticeshipStatus.Completed;
    
    public EstimatesTimeline Create(TimelineData data)
    {
        var standardCommitments = data.ApprenticeshipPaymentSummaries.Where(x => !x.IsTransfer).ToList();
        var standardPaymentSchedules = standardCommitments.Where(ApprenticeshipCanBeProjected).Select(PaymentScheduleGenerator.CreateFrom).ToList();
        
        var transferCommitments = data.ApprenticeshipPaymentSummaries.Where(x => x.IsTransfer).ToList();
        var transferPaymentSchedules = transferCommitments.Where(ApprenticeshipCanBeProjected).Select(PaymentScheduleGenerator.CreateFrom).ToList();
        
        var commitmentProjections = CommitmentsProjector.CreateProjection(data.StartingPeriod, data.Months, standardPaymentSchedules);
        var transferOutProjections = CommitmentsProjector.CreateProjection(data.StartingPeriod, data.Months, transferPaymentSchedules);
        var levyInProjections = LevyInProjector.CreateProjection(data.StartingPeriod, data.Months, data.HistoricLevyIn);

        var timeline = EstimatesTimeline.From(
            data.AccountId,
            data.StartingPeriod,
            data.Months,
            openingBalance: data.OpeningBalance,
            standardCommitments.Where(ApprenticeshipsHaveRemainingCost).Sum(c=>c.TotalCost - c.TotalPaid),
            transferCommitments.Where(ApprenticeshipsHaveRemainingCost).Sum(c=>c.TotalCost - c.TotalPaid),
            commitmentProjections,
            transferOutProjections,
            levyInProjections,
            expiredFunds,
            data.LevyFundsInForExpiry,
            data.PaymentFundsOutForExpiry,
            data.ShortTermExpiredFunds,
            data.LongTermExpiredFunds
        );

        CoInvestmentEstimator.Estimate(
            timeline,
            [..data.ApprenticeshipPaymentSummaries.Where(ApprenticeshipCanBeProjected)],
            [..standardPaymentSchedules, ..transferPaymentSchedules],
            data.OpeningBalance);

        return timeline;
    }
}