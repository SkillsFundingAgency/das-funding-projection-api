using FluentValidation;
using SFA.DAS.FundingProjection.Api.Models.Requests;

namespace SFA.DAS.FundingProjection.Api.Validators;

public class PutCommittedLearnerRequestValidator : AbstractValidator<PutCommittedLearnerRequest>
{
    public PutCommittedLearnerRequestValidator()
    {
        RuleFor(x => x.CommitmentId)
            .NotNull()
            .WithMessage("CommitmentId is required")
            .DependentRules(() =>
            {
                RuleFor(x => x.CommitmentId)
                    .Must(x => x > 0)
                    .WithMessage("CommitmentId must be greater than 0");
            });

        RuleFor(x => x.ApprenticeshipId)
            .GreaterThan(0)
            .WithMessage("ApprenticeshipId must be greater than 0");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("StartDate is required");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("EndDate is required")
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("EndDate must be greater than or equal to StartDate");
    }
}