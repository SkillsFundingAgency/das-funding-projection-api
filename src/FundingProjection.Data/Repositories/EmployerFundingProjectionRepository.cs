using Microsoft.EntityFrameworkCore;
using SFA.DAS.FundingProjection.Data.Models;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Data.Repositories;

public interface IEmployerFundingProjectionRepository : IReadRepository<EmployerFundingProjectionEntity, long>, IWriteRepository<EmployerFundingProjectionEntity, long>
{
    Task<List<EmployerFundingProjectionEntity>> GetByEmployerAccountIdAsync(long employerAccountId, int month, int year, CancellationToken cancellationToken);
    Task<List<EmployerFundingProjectionEntity>> GetTotalCostByMonthsAsync(long employerAccountId, int months = 12, CancellationToken cancellationToken = default);
}

public class EmployerFundingProjectionRepository(IFundingProjectionDataContext context) : IEmployerFundingProjectionRepository
{
    public async Task<List<EmployerFundingProjectionEntity>> GetByEmployerAccountIdAsync(long employerAccountId, int month, int year, CancellationToken cancellationToken)
    {
        return await context.EmployerFundingProjections
            .Where(x => x.EmployerAccountId == employerAccountId && x.CalendarPeriodMonth == month && x.CalendarPeriodYear == year)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EmployerFundingProjectionEntity>> GetTotalCostByMonthsAsync(long employerAccountId, int months, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var from = new DateTime(now.Year, now.Month, 1).AddMonths(-months);

        return await context.EmployerFundingProjections
            .Where(x => x.EmployerAccountId == employerAccountId
                        && (x.CalendarPeriodYear > from.Year
                            || (x.CalendarPeriodYear == from.Year
                                && x.CalendarPeriodMonth >= from.Month))
                        && (x.CalendarPeriodYear < now.Year
                            || (x.CalendarPeriodYear == now.Year
                                && x.CalendarPeriodMonth <= now.Month)))
            .OrderBy(x => x.CalendarPeriodYear)
            .ThenBy(x => x.CalendarPeriodMonth)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployerFundingProjectionEntity?> GetOneAsync(long key, CancellationToken cancellationToken)
    {
        return await context.EmployerFundingProjections.FindAsync([key], cancellationToken);
    }

    public async Task<UpsertResult<EmployerFundingProjectionEntity>> UpsertOneAsync(EmployerFundingProjectionEntity entity, CancellationToken cancellationToken)
    {
        var existingEntity = await context.EmployerFundingProjections
            .SingleOrDefaultAsync(x =>
                    x.EmployerAccountId == entity.EmployerAccountId &&
                    x.CalendarPeriodYear == entity.CalendarPeriodYear &&
                    x.CalendarPeriodMonth == entity.CalendarPeriodMonth,
                cancellationToken);

        if (existingEntity is null)
        {
            await context.EmployerFundingProjections.AddAsync(
                entity,
                cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
            return UpsertResult.Create(entity, true);
        }

        existingEntity.CommittedLearnerCostTotal = entity.CommittedLearnerCostTotal;
        existingEntity.CommittedTransferOutTotal = entity.CommittedTransferOutTotal;
        existingEntity.LastRecalculatedDate = entity.LastRecalculatedDate;

        await context.SaveChangesAsync(cancellationToken);
        return UpsertResult.Create(existingEntity, false);
    }

    public Task<bool> DeleteOneAsync(long key, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}