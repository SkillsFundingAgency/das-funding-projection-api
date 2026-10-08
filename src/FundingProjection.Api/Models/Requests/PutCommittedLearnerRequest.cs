using SFA.DAS.FundingProjection.Api.Models.Requests.Base;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.Models.Requests;

public sealed record PutCommittedLearnerRequest : CommittedLearnerRequestBase
{
    public long? CommitmentId { get; set; }
    public decimal Cost { get; set; }
    public const ImportStatus ImportStatus = Domain.Enums.ImportStatus.Imported;
}