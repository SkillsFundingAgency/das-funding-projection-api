using Microsoft.EntityFrameworkCore;
using SFA.DAS.FundingProjection.Data.Models;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Data.Repositories;

public interface ICommittedLearnerRepository : IReadRepository<CommittedLearnerEntity, Guid>,
    IWriteRepository<CommittedLearnerEntity, Guid>
{
    Task<List<CommittedLearnerEntity>> GetAllImportStatus(ImportStatus importStatus, CancellationToken cancellationToken);
}

public class CommittedLearnerRepository(IFundingProjectionDataContext dataContext) : ICommittedLearnerRepository
{
    public Task<CommittedLearnerEntity?> GetOneAsync(Guid key, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<UpsertResult<CommittedLearnerEntity>> UpsertOneAsync(CommittedLearnerEntity entity, CancellationToken cancellationToken)
    {
        var existingEntity = entity.EmployerAccountId == 0 ? null : await GetOneByApprenticeIdAsync(entity.ApprenticeshipId, cancellationToken);
        if (existingEntity is null)
        {
            await dataContext.CommittedLearners.AddAsync(entity, cancellationToken);
            await dataContext.SaveChangesAsync(cancellationToken);
            return UpsertResult.Create(entity, true);
        }

        existingEntity.EmployerAccountId = entity.EmployerAccountId;
        existingEntity.ApprenticeshipId = entity.ApprenticeshipId;
        existingEntity.CommitmentId = entity.CommitmentId;
        existingEntity.Cost = entity.Cost;
        existingEntity.StartDate = entity.StartDate;
        existingEntity.EndDate = entity.EndDate;
        existingEntity.PaymentStatus = entity.PaymentStatus;
        existingEntity.LastUpdatedDate = entity.LastUpdatedDate;
        existingEntity.ImportStatus = entity.ImportStatus;
        existingEntity.ImportedDate = entity.ImportedDate;

        dataContext.CommittedLearners.Update(existingEntity);
        await dataContext.SaveChangesAsync(cancellationToken);
        return UpsertResult.Create(existingEntity, false);
    }

    public Task<bool> DeleteOneAsync(Guid key, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private async Task<CommittedLearnerEntity?> GetOneByApprenticeIdAsync(long apprenticeId, CancellationToken cancellationToken)
    {
        return await dataContext.CommittedLearners
            .FirstOrDefaultAsync(x => x.ApprenticeshipId == apprenticeId,
                cancellationToken);
    }

    public async Task<List<CommittedLearnerEntity>> GetAllImportStatus(ImportStatus importStatus, CancellationToken cancellationToken)
    {
        return await dataContext.CommittedLearners
            .Where(x => x.ImportStatus == importStatus)
            .ToListAsync(cancellationToken);
    }
}