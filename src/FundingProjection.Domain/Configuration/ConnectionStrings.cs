using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.FundingProjection.Domain.Configuration;

[ExcludeFromCodeCoverage]
public class ConnectionStrings
{
    public required string SqlConnectionString { get; set; }
}