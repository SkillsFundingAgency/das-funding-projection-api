namespace SFA.DAS.FundingProjection.Api.Models.Responses;

public sealed record RecalculateFundingProjectionResponse(
    long TotalRecordsProcessed,
    long TotalRecordsUpdated,
    long TotalRecordsInserted);