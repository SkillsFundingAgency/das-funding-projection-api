using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace SFA.DAS.FundingProjection.Api.NServiceBus;

[ExcludeFromCodeCoverage]
public static class RuleNameShortener
{
    private const int AzureServiceBusRuleNameMaxLength = 50;

    public static string Shorten(Type arg)
    {
        var fullName = arg.FullName!;
        if (fullName.Length <= AzureServiceBusRuleNameMaxLength)
        {
            return fullName;
        }
        var bytes = System.Text.Encoding.Default.GetBytes(fullName);
        return new Guid(MD5.HashData(bytes)).ToString();
    }
}