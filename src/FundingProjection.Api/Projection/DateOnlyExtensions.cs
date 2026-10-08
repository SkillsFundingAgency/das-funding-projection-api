namespace SFA.DAS.FundingProjection.Api.Projection;

public static class DateOnlyExtensions 
{
    public static DateOnly ToPeriod(this DateOnly dateTime) => new(dateTime.Year, dateTime.Month, 1);
}