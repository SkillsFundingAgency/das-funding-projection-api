using SFA.DAS.FundingProjection.Api.Models.Requests.Base;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.Models.Requests;

public sealed record PostCommittedLearnerRequest : CommittedLearnerRequestBase
{
    public const ImportStatus ImportStatus = Domain.Enums.ImportStatus.Pending;
}