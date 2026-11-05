using SFA.DAS.FundingProjection.Api.Projection;

namespace FundingProjection.Api.Testing.DataCreation;

public static class HistoricLevy 
{
    private static DateOnly Now => DateTime.UtcNow.ToPeriod();
    
    public static List<LevyInMonthSummary> Generate(decimal? fixedMonthlyAmount = null, bool includeCurrentMonth = false) => Generate(Now, includeCurrentMonth, fixedMonthlyAmount);
    public static List<LevyInMonthSummary> Generate(DateOnly? startingPoint, decimal? fixedMonthlyAmount = null, bool includeCurrentMonth = false) => Generate(startingPoint ?? Now, includeCurrentMonth, fixedMonthlyAmount);
    
    public static List<LevyInMonthSummary> Generate(DateOnly startingPoint, bool includeCurrentMonth = false, decimal? fixedMonthlyAmount = null)
    {
        var r = new Random();
        var months = includeCurrentMonth ? 12 : 11;
        var startPeriod = includeCurrentMonth ? startingPoint : startingPoint.AddMonths(-1).ToPeriod();
        var endPeriod = startPeriod.AddMonths(-months);
        
        var results = new List<LevyInMonthSummary>();
        for (var period = startPeriod; period >= endPeriod; period = period.AddMonths(-1))
        {
            results.Add(new LevyInMonthSummary(period, fixedMonthlyAmount ?? r.Next(1000, 10000)));
        }

        return results;
    }
}