using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Api.Models.Mappers;

public static class CommittedLearnerExtensions
{
    public static CommittedLearnerEntity ToEntity(this PutCommittedLearnerRequest request, Guid id, long employerAccountId)
    {
        return new CommittedLearnerEntity
        {
            Id = id,
            EmployerAccountId = employerAccountId,
            PaymentStatus = request.PaymentStatus,
            Cost = request.Cost,
            StartDate = DateOnly.FromDateTime(request.StartDate),
            EndDate = DateOnly.FromDateTime(request.EndDate),
            ApprenticeshipId = request.ApprenticeshipId,
            CommitmentId = request.CommitmentId,
            CreatedDate = request.CreatedOn,
            LastUpdatedDate = request.UpdatedOn,
            ImportedDate = DateTime.UtcNow,
            ImportStatus = PutCommittedLearnerRequest.ImportStatus,
        };
    }

    public static CommittedLearnerEntity ToEntity(this PostCommittedLearnerRequest request, long employerAccountId)
    {
        return new CommittedLearnerEntity
        {
            Id = Guid.NewGuid(),
            EmployerAccountId = employerAccountId,
            PaymentStatus = request.PaymentStatus,
            StartDate = DateOnly.FromDateTime(request.StartDate),
            EndDate = DateOnly.FromDateTime(request.EndDate),
            ApprenticeshipId = request.ApprenticeshipId,
            CreatedDate = request.CreatedOn,
            LastUpdatedDate = request.UpdatedOn,
            ImportStatus = PostCommittedLearnerRequest.ImportStatus,
        };
    }

    public static GetCommittedLearnerResponse ToResponse(this CommittedLearnerEntity entity)
    {
        return new GetCommittedLearnerResponse
        {
            Id = entity.Id,
            ApprenticeshipId = entity.ApprenticeshipId,
            CommitmentId = entity.CommitmentId,
            Cost = entity.Cost,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            CreatedOn = entity.CreatedDate,
            UpdatedOn = entity.LastUpdatedDate,
            ImportedOn = entity.ImportedDate,
            PaymentStatus = entity.PaymentStatus,
            ImportStatus = entity.ImportStatus,
        };
    }
}