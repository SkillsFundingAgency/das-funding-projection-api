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
        var existingEntity = entity.EmployerAccountId == 0 ? null : await GetOneAsync(entity.EmployerAccountId, cancellationToken);
        if (existingEntity is null)
        {
            await context.EmployerFundingProjections.AddAsync(entity, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return UpsertResult.Create(entity, true);
        }

        context.SetValues(existingEntity, entity);
        await context.SaveChangesAsync(cancellationToken);
        return UpsertResult.Create(entity, false);
    }

    public Task<bool> DeleteOneAsync(long key, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}