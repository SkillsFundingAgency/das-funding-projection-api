using SFA.DAS.FundingProjection.Data.Models;

namespace SFA.DAS.FundingProjection.Data;

public interface IWriteRepository<TEntity, in TKey>
{
    Task<UpsertResult<TEntity>> UpsertOneAsync(TEntity entity, CancellationToken cancellationToken);
    Task<bool> DeleteOneAsync(TKey key, CancellationToken cancellationToken);
}
