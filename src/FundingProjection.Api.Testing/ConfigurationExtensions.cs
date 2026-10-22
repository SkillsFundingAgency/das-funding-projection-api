using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;

namespace FundingProjection.Api.Testing;

public static class ConfigurationExtensions
{
    public static string GetDbSchemaName(this IConfiguration? configuration, [CallerMemberName] string callerName = "")
    {
        if (configuration is null)
        {
            throw new Exception($"{callerName} passed a null configuration instance");
        }

        var result = configuration["TestingDbSchemaName"];
        return string.IsNullOrEmpty(result)
            ? throw new Exception($"{callerName}: Failed to get integration test database schema name from configuration")
            : result!;
    }
}