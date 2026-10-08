namespace SFA.DAS.FundingProjection.Data.Models;

public record UpsertResult<T>(T Entity, bool Created, bool? StatusChanged = null);

public static class UpsertResult
{
    public static UpsertResult<TResult> Create<TResult>(TResult entity, bool created, bool? statusChanged = null)
    {
        return new UpsertResult<TResult>(entity, created, statusChanged);
    }
}