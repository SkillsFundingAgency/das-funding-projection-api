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
        var totalRecordsUpdated = 0;
        var totalRecordsProcessed = 0;
        var totalRecordsInserted = 0;

        var affectedEmployerAccountIds = await committedLearnerRepository
                .GetAll()
                .Where(x => x.LastUpdatedDate >= cutOffDateTime)
                .Select(x => x.EmployerAccountId)
                .Distinct()
                .ToListAsync(cancellationToken);

        if (affectedEmployerAccountIds.Count == 0)
        {
            return new RecalculatedResponse(totalRecordsProcessed, totalRecordsUpdated, totalRecordsInserted);
        }

        var learners =
            await committedLearnerRepository
                .GetAll()
                .Where(x => affectedEmployerAccountIds.Contains(x.EmployerAccountId))
                .ToListAsync(cancellationToken);

        var projections = learners
            .GroupBy(x => new
            {
                EmployerAccountId = x.EmployerAccountId,
                Year = x.StartDate.Year,
                Month = x.StartDate.Month
            })
            .Select(g => new EmployerFundingProjectionEntity
            {
                Id = Guid.NewGuid(),
                EmployerAccountId = g.Key.EmployerAccountId,
                CommittedLearnerCostTotal = g.Sum(x => x.Cost),
                CommittedTransferOutTotal = 0m,
                CalendarPeriodMonth = g.Key.Month,
                CalendarPeriodYear = g.Key.Year,
                LastRecalculatedDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            })
            .OrderBy(x => x.EmployerAccountId)
            .ThenBy(x => x.CalendarPeriodYear)
            .ThenBy(x => x.CalendarPeriodMonth)
            .ToList();

        foreach (var employerFundingProjectionEntity in projections)
        {
            var upsertResult = await employerFundingProjectionRepository.UpsertOneAsync(
                employerFundingProjectionEntity,
                cancellationToken);
            
            totalRecordsProcessed++;

            if (upsertResult.Created)
                totalRecordsInserted++;
            else
                totalRecordsUpdated++;
        }

        return new RecalculatedResponse(totalRecordsProcessed, totalRecordsUpdated, totalRecordsInserted);
    }
}