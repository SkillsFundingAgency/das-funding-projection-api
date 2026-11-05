namespace SFA.DAS.FundingProjection.Domain.Events;

public sealed record ImportLearnersCostEvent(Guid JobId,
    long AccountId,
    DateTime LastSuccessfulImportDate);