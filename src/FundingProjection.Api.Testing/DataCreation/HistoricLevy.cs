using SFA.DAS.FundingProjection.Api.Projection;

namespace FundingProjection.Api.Testing.DataCreation;

public static class HistoricLevy 
{
    public static List<LevyInMonthSummary> Generate(decimal? fixedMonthlyAmount = null, bool includeCurrentMonth = false) => Generate(DateTime.UtcNow, includeCurrentMonth, fixedMonthlyAmount);
    
    public static List<LevyInMonthSummary> Generate(DateTime startingPoint, bool includeCurrentMonth = false, decimal? fixedMonthlyAmount = null)
    {
        var r = new Random();
        var months = includeCurrentMonth ? 12 : 11;
        var startPeriod = includeCurrentMonth ? startingPoint.ToPeriod() : startingPoint.AddMonths(-1).ToPeriod();
        var endPeriod = startPeriod.AddMonths(-months);
        
        var results = new List<LevyInMonthSummary>();
        for (var period = startPeriod; period >= endPeriod; period = period.AddMonths(-1))
        {
            results.Add(new LevyInMonthSummary(period, fixedMonthlyAmount ?? r.Next(1000, 10000)));
        }

        return results;
    }
}