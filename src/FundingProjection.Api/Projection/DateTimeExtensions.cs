namespace SFA.DAS.FundingProjection.Api.Projection;

public static class DateTimeExtensions 
{
    public static DateOnly ToPeriod(this DateTime dateTime) => new(dateTime.Year, dateTime.Month, 1);
}