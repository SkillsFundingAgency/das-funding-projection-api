using System.Text.Json.Serialization;

namespace SFA.DAS.FundingProjection.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LearnerCostStatus
{
    Active = 1,
    Completed = 2,
    Withdrawn = 3,
    Paused = 4
}