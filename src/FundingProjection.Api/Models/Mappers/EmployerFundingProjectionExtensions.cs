using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Api.Models.Mappers;

public static class EmployerFundingProjectionExtensions
{
    public static GetEmployerFundingProjectionResponse ToGetResponse(this EmployerFundingProjectionEntity entity) => new()
    {
            EmployerAccountId = entity.EmployerAccountId,
            CommittedLearnerCostTotal = entity.CommittedLearnerCostTotal,
            CommittedTransferOutTotal = entity.CommittedTransferOutTotal,
            LastRecalculatedDate = entity.LastRecalculatedDate,
            CreatedDate = entity.CreatedDate
    };
}