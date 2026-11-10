using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.Models;

public static class ApprenticeshipPaymentSummaryEntityExtensions
{
    extension(ApprenticeshipPaymentSummaryEntity entity)
    {
        public int AgeAtStart => CalculateAge(entity.Dob, entity.StartDate.ToDateTime(TimeOnly.MinValue));

        public bool CanBeProjected => entity.Status is ApprenticeshipStatus.Live
            or ApprenticeshipStatus.WaitingToStart
            or ApprenticeshipStatus.Completed;

        // the completion payment is made the month after the planned end date
        public DateOnly LastPaymentPeriod => entity.EndDate.AddMonths(1).ToPeriod();
    }
    
    internal static int CalculateAge(DateOnly dob, DateTime startDate)
    {
        // adapted from https://github.com/arman-g/AgeCalculator/blob/main/AgeCalculator/Models/Age.cs
        var fromDate = dob.ToDateTime(TimeOnly.MinValue);
        const int totalMonths = 12;
        var remainderDay = fromDate.TimeOfDay > startDate.TimeOfDay ? 1 : 0;
        var remainderMonth = fromDate.Day > startDate.Day - remainderDay ? 1 : 0;
        if (fromDate.Month == startDate.Month)
        {
            return startDate.Year - fromDate.Year - remainderMonth;
        }
        
        var months = fromDate.Month > startDate.Month ? totalMonths : 0;
        return startDate.Year - fromDate.Year - months / totalMonths;
    }
}