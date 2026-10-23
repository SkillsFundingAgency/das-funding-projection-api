using Microsoft.EntityFrameworkCore;
using SFA.DAS.FundingProjection.Data.Models;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Data.Repositories;
public interface IApprenticeshipPaymentSummariesRepository : IReadRepository<ApprenticeshipPaymentSummaryEntity, long>, IWriteRepository<ApprenticeshipPaymentSummaryEntity, long>
{
    Task<List<ApprenticeshipPaymentSummaryEntity>> GetApprenticeshipPaymentSummariesAsync(long accountId, CancellationToken cancellationToken);
}
public class ApprenticeshipPaymentSummariesRepository(IFundingProjectionDataContext context) : IApprenticeshipPaymentSummariesRepository
{
    public async Task<ApprenticeshipPaymentSummaryEntity?> GetOneAsync(long key, CancellationToken cancellationToken)
    {
        return await context.ApprenticeshipPaymentSummaries.FindAsync([key], cancellationToken);
    }

    public async Task<UpsertResult<ApprenticeshipPaymentSummaryEntity>> UpsertOneAsync(ApprenticeshipPaymentSummaryEntity entity, CancellationToken cancellationToken)
    {
        var existingEntity = await context.ApprenticeshipPaymentSummaries
            .SingleOrDefaultAsync(x =>
                    x.AccountId == entity.AccountId &&
                    x.ApprenticeshipId == entity.ApprenticeshipId,
                cancellationToken);

        if (existingEntity is null)
        {
            await context.ApprenticeshipPaymentSummaries.AddAsync(
                entity,
                cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
            return UpsertResult.Create(entity, true);
        }

        existingEntity.IsTransfer = entity.IsTransfer;
        existingEntity.LastPaymentAmount = entity.LastPaymentAmount;
        existingEntity.LastPaymentDate = entity.LastPaymentDate;
        existingEntity.LastUpdatedDate = entity.LastUpdatedDate;
        existingEntity.Uln = entity.Uln;
        existingEntity.AgeAtStart = entity.AgeAtStart;
        existingEntity.Status = entity.Status;
        existingEntity.TotalCost = entity.TotalCost;
        existingEntity.TotalPaid = entity.TotalPaid;
        existingEntity.StartDate = entity.StartDate;
        existingEntity.EndDate = entity.EndDate;

        await context.SaveChangesAsync(cancellationToken);
        return UpsertResult.Create(existingEntity, false);
    }

    public Task<bool> DeleteOneAsync(long key, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    public async Task<List<ApprenticeshipPaymentSummaryEntity>> GetApprenticeshipPaymentSummariesAsync(long accountId, CancellationToken cancellationToken)
    {
        return await context.ApprenticeshipPaymentSummaries
            .Where(x => x.AccountId == accountId)
            .ToListAsync(cancellationToken);
    }
}
