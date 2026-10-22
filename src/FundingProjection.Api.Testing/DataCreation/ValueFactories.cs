namespace FundingProjection.Api.Testing.DataCreation;

public static class ApprenticeshipIdGenerator
{
    private static readonly Lock LockObject = new(); 
    private static long _currentValue = 200000L;

    public static long Next()
    {
        lock (LockObject)
        {
            return _currentValue++;
        }
    }
}

public static class UlnGenerator
{
    private static readonly Lock LockObject = new(); 
    private static long _currentValue = 10000L;

    public static long Next()
    {
        lock (LockObject)
        {
            return _currentValue++;
        }
    }
}

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