namespace SFA.DAS.FundingProjection.Data;

public interface IReadRepository<TEntity, in TKey>
{
    Task<TEntity?> GetOneAsync(TKey key, CancellationToken cancellationToken);
}