using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FundingProjection.Api.Testing;

public static class Measure
{
    private const int Tolerance = 2000;
    
    private static void CheckTime(long elapsedMilliseconds, long millisecondsTimeout, string callerName)
    {
        if (elapsedMilliseconds > millisecondsTimeout)
        {
            Assert.Fail($"{callerName}: Measured function took {elapsedMilliseconds}ms which is longer than the {millisecondsTimeout}ms timeout specified.");
        }
    }
    
    public static async Task<T> ThisAsync<T>(Func<Task<T>> func, int millisecondsTimeout = Tolerance, [CallerMemberName] string callerName = "")
    {
        var sw = Stopwatch.StartNew();
        var result = await func();
        sw.Stop();
        CheckTime(sw.ElapsedMilliseconds, millisecondsTimeout, callerName);
        return result;
    }
}