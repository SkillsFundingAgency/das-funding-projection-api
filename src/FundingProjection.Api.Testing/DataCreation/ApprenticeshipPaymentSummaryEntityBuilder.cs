using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace FundingProjection.Api.Testing.DataCreation;

public class ApprenticeshipPaymentSummaryEntityBuilder(ApprenticeshipPaymentSummaryEntity entity)
{
    private bool _statusSet;
    private int _ageAtStart = TestData.DefaultAge;
    private readonly DateOnly _now = DateTime.UtcNow.ToPeriod();
    
    public ApprenticeshipPaymentSummaryEntityBuilder Started(int monthsAgo = 0) => StartsIn(-monthsAgo);
    
    public ApprenticeshipPaymentSummaryEntityBuilder StartsIn(int months = 0)
    {
        entity.StartDate = DateTime.UtcNow.ToPeriod().AddMonths(months);
        entity.EndDate = entity.StartDate.AddMonths(TestData.DefaultLength-1);
        return this;
    }

    public ApprenticeshipPaymentSummaryEntityBuilder Start(DateOnly date)
    {
        entity.StartDate = date;
        entity.EndDate = entity.StartDate.AddMonths(TestData.DefaultLength-1);
        return this;
    }

    public ApprenticeshipPaymentSummaryEntityBuilder Length(int months)
    {
        entity.EndDate = entity.StartDate.AddMonths(months-1);
        return this;
    }

    public ApprenticeshipPaymentSummaryEntityBuilder TotalCost(decimal value)
    {
        entity.TotalCost = value;
        return this;
    }
    
    public ApprenticeshipPaymentSummaryEntityBuilder TotalPaid(decimal value)
    {
        entity.TotalPaid = value;
        return this;
    }
    
    public ApprenticeshipPaymentSummaryEntityBuilder Live()
    {
        entity.Status = ApprenticeshipStatus.Live;
        _statusSet = true;
        return this;
    }
    
    public ApprenticeshipPaymentSummaryEntityBuilder Paused()
    {
        entity.Status = ApprenticeshipStatus.Paused;
        _statusSet = true;
        return this;
    }
    
    public ApprenticeshipPaymentSummaryEntityBuilder Stopped()
    {
        entity.Status = ApprenticeshipStatus.Stopped;
        _statusSet = true;
        return this;
    }
    
    public ApprenticeshipPaymentSummaryEntityBuilder Transferred()
    {
        entity.IsTransfer = true;
        return this;
    }

    public ApprenticeshipPaymentSummaryEntityBuilder LastPayment(decimal amount, DateOnly when)
    {
        entity.LastPaymentAmount = amount;
        entity.LastPaymentDate = when;
        return this;
    }

    public ApprenticeshipPaymentSummaryEntityBuilder Age(int age)
    {
        _ageAtStart = age;
        return this;
    }

    public ApprenticeshipPaymentSummaryEntity Build()
    {
        if (!_statusSet)
        {
            if (entity.EndDate < _now)
            {
                if (entity.EndDate.AddMonths(1) == _now)
                {
                    entity.Status = entity.LastPaymentDate == _now
                        ? ApprenticeshipStatus.Completed
                        : ApprenticeshipStatus.Live;
                }
                else
                {
                    entity.Status = ApprenticeshipStatus.Completed;
                }
            } 
            else if (entity.StartDate <= _now && _now <= entity.EndDate)
            {
                entity.Status = ApprenticeshipStatus.Live;
            }
            else if (entity.StartDate > _now)
            {
                entity.Status = ApprenticeshipStatus.WaitingToStart;
            }
        }
        
        entity.Dob = entity.StartDate.AddYears(-_ageAtStart);
        return entity;
    }
}