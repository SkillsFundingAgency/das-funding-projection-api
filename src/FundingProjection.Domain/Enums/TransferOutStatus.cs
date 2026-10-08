using System.Text.Json.Serialization;

namespace SFA.DAS.FundingProjection.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransferOutStatus
{
    Pending = 1,
    Completed = 2,
    Stopped = 3
}