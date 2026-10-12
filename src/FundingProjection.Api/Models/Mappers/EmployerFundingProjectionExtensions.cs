using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Data.Models;

namespace SFA.DAS.FundingProjection.Api.Models.Mappers;

public static class EmployerFundingProjectionExtensions
{
    public static RecalculateFundingProjectionResponse ToRecalculateFundingProjectionResponse(this RecalculatedResponse response) => new(
        TotalRecordsProcessed: response.TotalRecordsProcessed,
        TotalRecordsUpdated: response.TotalRecordsUpdated,
        TotalRecordsInserted: response.TotalRecordsInserted
    );
}