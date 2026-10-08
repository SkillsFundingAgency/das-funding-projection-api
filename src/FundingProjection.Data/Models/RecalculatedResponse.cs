namespace SFA.DAS.FundingProjection.Data.Models;

public sealed record RecalculatedResponse(int TotalRecordsProcessed,
    int TotalRecordsUpdated,
    int TotalRecordsInserted);