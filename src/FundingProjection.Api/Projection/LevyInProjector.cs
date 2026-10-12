namespace SFA.DAS.FundingProjection.Api.Projection;

public class LevyInProjector
{
    public static List<LevyInMonthSummary> CreateProjection(DateOnly now, int months, List<LevyInMonthSummary> historicLevyIn)
    {
        var pointInTime = now.ToPeriod();
        var results = new List<LevyInMonthSummary>();
        
        for (var i = 0; i < months; i++)
        {
            LevyInMonthSummary? levyData = historicLevyIn
                .Where(x => x.Period.Month == pointInTime.Month)
                .OrderByDescending(x => x.Period.Year)
                .FirstOrDefault();
            results.Add(new LevyInMonthSummary(new DateOnly(pointInTime.Year, pointInTime.Month, 1), levyData?.Amount ?? 0m));
            pointInTime = pointInTime.AddMonths(1);
        }

        return results;
    }
}