namespace FundingProjection.Api.Testing.DataCreation;

public static class AccountIdGenerator
{
    private static readonly Lock LockObject = new(); 
    private static long _currentValue = 1L;

    public static long Next()
    {
        lock (LockObject)
        {
            return _currentValue++;
        }
    }
}