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