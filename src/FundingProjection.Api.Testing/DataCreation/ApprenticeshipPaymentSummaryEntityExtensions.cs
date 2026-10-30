using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace FundingProjection.Api.Testing.DataCreation;

public static class ApprenticeshipPaymentSummaryEntityExtensions
{
    extension(ApprenticeshipPaymentSummaryEntity)
    {
        public static ApprenticeshipPaymentSummaryEntity Create(long accountId, Action<ApprenticeshipPaymentSummaryEntityBuilder>? options = null)
        {
            var now =  DateTime.UtcNow.ToPeriod();
            var entity = new ApprenticeshipPaymentSummaryEntity()
            {
                AccountId = accountId,
                Uln = UlnGenerator.Next(),
                ApprenticeshipId = ApprenticeshipIdGenerator.Next(),
                StartDate = now,
                EndDate = now.AddMonths(TestData.DefaultLength-1), // -1 because months are inclusive
                TotalCost = TestData.DefaultTotalCost,
                TotalPaid = 0,
                Dob = now.AddYears(-TestData.DefaultAge),
                IsTransfer = false,
                Status = ApprenticeshipStatus.Live,
                ProviderOrder = 0,
                ApprovalDate = now.ToDateTime(TimeOnly.MinValue),
                LearningType = TestData.DefaultLearningType,
            };
            var builder = new ApprenticeshipPaymentSummaryEntityBuilder(entity);
            options?.Invoke(builder);
            return builder.Build();
        }
    }
}