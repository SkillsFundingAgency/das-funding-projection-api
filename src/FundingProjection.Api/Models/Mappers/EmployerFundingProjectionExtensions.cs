using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Data.Models;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Api.Models.Mappers;

public static class EmployerFundingProjectionExtensions
{
    public static MonthlyFundingBreakdown ToGetResponse(this EmployerFundingProjectionEntity entity) => new()
    {
            EmployerAccountId = entity.EmployerAccountId,
            Month = entity.CalendarPeriodMonth,
            Year = entity.CalendarPeriodYear,
            CommittedLearnerCost = entity.CommittedLearnerCostTotal,
            CommittedTransferOut = entity.CommittedTransferOutTotal,
    };

    public static RecalculateFundingProjectionResponse ToRecalculateFundingProjectionResponse(this RecalculatedResponse response) => new(
        TotalRecordsProcessed: response.TotalRecordsProcessed,
        TotalRecordsUpdated: response.TotalRecordsUpdated,
        TotalRecordsInserted: response.TotalRecordsInserted
    );
}