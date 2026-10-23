using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Api.Models.Mappers;

public static class ApprenticeshipPaymentSummaryExtensions
{
    public static ApprenticeshipPaymentSummaryEntity ToEntity(this PostApprenticeshipPaymentSummaryRequest summary,
        long accountId,
        long apprenticeshipId)
    {
        return new ApprenticeshipPaymentSummaryEntity
        {
            AccountId = accountId,
            ApprenticeshipId = apprenticeshipId,
            Status = summary.Status,
            Uln = summary.Uln,
            AgeAtStart = summary.AgeAtStart,
            TotalCost = summary.TotalCost,
            TotalPaid = summary.TotalPaid,
            StartDate = summary.StartDate,
            EndDate = summary.EndDate,
            LastPaymentAmount = summary.LastPaymentAmount,
            IsTransfer = summary.IsTransfer,
            LastUpdatedDate = summary.LastUpdatedDate,
            LastPaymentDate = summary.LastPaymentDate
        };
    }

    public static ApprenticeshipPaymentSummaryResponse ToResponse(this ApprenticeshipPaymentSummaryEntity summary)
    {
        return new ApprenticeshipPaymentSummaryResponse
        {
            AccountId = summary.AccountId,
            ApprenticeshipId = summary.ApprenticeshipId,
            Status = summary.Status,
            Uln = summary.Uln,
            AgeAtStart = summary.AgeAtStart,
            TotalCost = summary.TotalCost,
            TotalPaid = summary.TotalPaid,
            StartDate = summary.StartDate,
            EndDate = summary.EndDate,
            LastPaymentAmount = summary.LastPaymentAmount,
            IsTransfer = summary.IsTransfer,
            LastUpdatedDate = summary.LastUpdatedDate,
            LastPaymentDate = summary.LastPaymentDate
        };
    }
}