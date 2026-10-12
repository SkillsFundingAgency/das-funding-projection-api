using FluentValidation;
using SFA.DAS.FundingProjection.Api.Models.Requests;

namespace SFA.DAS.FundingProjection.Api.Validators;

public class PostCommittedLearnerRequestValidator : AbstractValidator<PostCommittedLearnerRequest>
{
    public PostCommittedLearnerRequestValidator()
    {
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