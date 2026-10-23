namespace SFA.DAS.FundingProjection.Api.Projection;

public static class LevyInProjector
{
    public static List<LevyInMonthSummary> CreateProjection(DateOnly now, int months, List<LevyInMonthSummary> historicLevyIn)
    {
        var latestByMonth = historicLevyIn
            .GroupBy(x => x.Period.Month)
            .ToDictionary(
                g => g.Key,
                g => g.MaxBy(x => x.Period.Year)!.Amount);

        var pointInTime = now.ToPeriod();
        var results = new List<LevyInMonthSummary>(months);

        for (var i = 0; i < months; i++)
        {
            var amount = latestByMonth.GetValueOrDefault(pointInTime.Month, 0m);
            results.Add(new LevyInMonthSummary(
                new DateOnly(pointInTime.Year, pointInTime.Month, 1), amount));
            pointInTime = pointInTime.AddMonths(1);
        }

        return results;
    }
}