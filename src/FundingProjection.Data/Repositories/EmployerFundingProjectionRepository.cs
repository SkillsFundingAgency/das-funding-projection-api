using Microsoft.EntityFrameworkCore;
using SFA.DAS.FundingProjection.Data.Models;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Data.Repositories;

public interface IEmployerFundingProjectionRepository : IReadRepository<EmployerFundingProjectionEntity, long>, IWriteRepository<EmployerFundingProjectionEntity, long>
{
    Task<EmployerFundingProjectionEntity?> GetByEmployerAccountIdAsync(long employerAccountId, CancellationToken cancellationToken);
}

public class EmployerFundingProjectionRepository(IFundingProjectionDataContext context) : IEmployerFundingProjectionRepository
{
    public async Task<EmployerFundingProjectionEntity?> GetByEmployerAccountIdAsync(long employerAccountId, CancellationToken cancellationToken)
    {
        return await context.EmployerFundingProjections
            .FirstOrDefaultAsync(x => x.EmployerAccountId == employerAccountId,
                cancellationToken);
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