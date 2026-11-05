using System.Diagnostics.CodeAnalysis;
using SFA.DAS.FundingProjection.Api.Models;

namespace SFA.DAS.FundingProjection.Api.Projection;

public static class CopaymentRules
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static class Dates
    {
        public static readonly DateOnly May1st2017 = new(2017, 5, 1);
        public static readonly DateOnly April1st2019 = new(2019, 4, 1);
        public static readonly DateOnly August1st2026 = new(2026, 8, 1);
    }
    
    public static readonly CopaymentRule DefaultPercent0 = CopaymentRule.Create(_ => true, _ => 0m); 
    public static readonly CopaymentRule Percent25 = CopaymentRule.Create(x => x.StartDate >= Dates.August1st2026 && x.AgeAtStart >= 25, x => x * 0.25m); 
    public static readonly CopaymentRule Percent10 = CopaymentRule.Create(x => x.StartDate >= Dates.May1st2017 && x.StartDate < Dates.April1st2019, x => x * 0.1m); 
    public static readonly CopaymentRule Percent5 = CopaymentRule.Create(x => x.StartDate >= Dates.April1st2019 && x.StartDate < Dates.August1st2026, x => x * 0.05m); 
}