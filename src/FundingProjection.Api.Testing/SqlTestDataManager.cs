using SFA.DAS.FundingProjection.Data;

namespace FundingProjection.Api.Testing;

public class SqlTestDataManager(FundingProjectionDataContext dataContext)
{
    public async Task<T> AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        await dataContext.AddAsync(entity, cancellationToken);
        await dataContext.SaveChangesAsync(cancellationToken);
        return entity;
    }
    
    public async Task<List<T>> AddManyAsync<T>(List<T> entities, CancellationToken cancellationToken = default) where T : class
    {
        await dataContext.AddRangeAsync(entities, cancellationToken);
        await dataContext.SaveChangesAsync(cancellationToken);
        return entities;
    }

    public async Task<T?> GetAsync<T>(object id) where T : class
    {
        return await dataContext.Set<T>().FindAsync(id);
    }
}