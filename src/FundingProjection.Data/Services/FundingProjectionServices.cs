using Microsoft.EntityFrameworkCore;
using SFA.DAS.FundingProjection.Data.Models;
using SFA.DAS.FundingProjection.Data.Repositories;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Data.Services;

public interface IFundingProjectionServices
{
    Task<RecalculatedResponse> UpdateProjection(DateTime cutOffDateTime, CancellationToken cancellationToken);
}
public class FundingProjectionServices(
    ICommittedLearnerRepository committedLearnerRepository,
    IEmployerFundingProjectionRepository employerFundingProjectionRepository)
    : IFundingProjectionServices
{
    public async Task<RecalculatedResponse> UpdateProjection(DateTime cutOffDateTime, CancellationToken cancellationToken)
    {
        var affectedEmployerAccountIds = await committedLearnerRepository
            .GetAll()
            .Where(x => x.LastUpdatedDate >= cutOffDateTime)
            .Select(x => x.EmployerAccountId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (affectedEmployerAccountIds.Count == 0)
            return new RecalculatedResponse(0, 0, 0);

        var now = DateTime.UtcNow;

        var learners = await committedLearnerRepository
            .GetAll()
            .Where(x => affectedEmployerAccountIds.Contains(x.EmployerAccountId))
            .ToListAsync(cancellationToken);

        var projections = learners
            .GroupBy(x => new { x.EmployerAccountId, x.StartDate.Year, x.StartDate.Month })
            .Select(g => new EmployerFundingProjectionEntity
            {
                Id = Guid.NewGuid(),
                EmployerAccountId = g.Key.EmployerAccountId,
                CommittedLearnerCostTotal = g.Sum(x => x.Cost),
                CommittedTransferOutTotal = 0m,
                CalendarPeriodMonth = g.Key.Month,
                CalendarPeriodYear = g.Key.Year,
                LastRecalculatedDate = now,
                CreatedDate = now
            })
            .OrderBy(x => x.EmployerAccountId)
            .ThenBy(x => x.CalendarPeriodYear)
            .ThenBy(x => x.CalendarPeriodMonth)
            .ToList();

        var totalRecordsInserted = 0;

        foreach (var projection in projections)
        {
            var result = await employerFundingProjectionRepository.UpsertOneAsync(projection, cancellationToken);
            if (result.Created) totalRecordsInserted++;
        }

        return new RecalculatedResponse(
            TotalRecordsProcessed: projections.Count,
            TotalRecordsUpdated: projections.Count - totalRecordsInserted,
            TotalRecordsInserted: totalRecordsInserted);
    }
}