using SFA.DAS.EmployerFinance.Types.Models;
using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Projection;

public class WhenCalculatingCoinvestment
{
    [Test]
    [MoqInlineAutoData(25)]
    [MoqInlineAutoData(26)]
    [MoqInlineAutoData(27)]
    public void Then_If_A_Leaner_Starts_On_August_1st_Or_Later_And_They_Are_25_Or_Older_At_The_Start_Of_The_Apprenticeship_Then_The_Copayment_Amount_Is_25_Percent(
        int ageAtStart,
        long accountId,
        Mock<IExpiredFunds> expiredFunds)
    {
        // arrange
        var now = DateOnly.FromDateTime(DateTime.UtcNow).ToPeriod();
        List<ApprenticeshipPaymentSummaryEntity> summaries =
        [
            new()
            {
                ApprenticeshipId = 1,
                AccountId = accountId,
                TotalCost = 375m,
                TotalPaid = 0m,
                StartDate = now.AddMonths(-1),
                EndDate = now.AddMonths(1),
                Status = ApprenticeshipStatus.Live,
                IsTransfer = false,
                Dob = now.AddMonths(-1).AddYears(-ageAtStart),
            }
        ];

        var data = new TimelineData
        {
            AccountId = accountId,
            Months = 3,
            OpeningBalance = 0,
            StartingPeriod = now,
            ApprenticeshipPaymentSummaries = summaries,
        };
        
        var sut = new TimelineGenerator(expiredFunds.Object, new CoInvestmentEstimator(new ApprenticeshipOrderingStrategy()));

        // act
        var timeline = sut.Create(data);

        // assert
        timeline.Projections[0].CoInvestmentCosts.Should().Be(25m);
        timeline.Projections[1].CoInvestmentCosts.Should().Be(25m);
        timeline.Projections[2].CoInvestmentCosts.Should().Be(43.75m);
    }
    
    [Test]
    [MoqInlineAutoData(23)]
    [MoqInlineAutoData(24)]
    public void Then_If_A_Leaner_Starts_On_August_1st_Or_Later_And_They_Are_Under_25_At_The_Start_Of_The_Apprenticeship_Then_The_Copayment_Amount_Is_0_Percent(
        int ageAtStart,
        long accountId,
        Mock<IExpiredFunds> expiredFunds)
    {
        // arrange
        var now = DateOnly.FromDateTime(DateTime.UtcNow).ToPeriod();
        List<ApprenticeshipPaymentSummaryEntity> summaries =
        [
            new()
            {
                ApprenticeshipId = 1,
                AccountId = accountId,
                TotalCost = 375m,
                TotalPaid = 0m,
                StartDate = now.AddMonths(-1),
                EndDate = now.AddMonths(1),
                Status = ApprenticeshipStatus.Live,
                IsTransfer = false,
                Dob = now.AddMonths(-1).AddYears(-ageAtStart),
            }
        ];

        var data = new TimelineData
        {
            AccountId = accountId,
            Months = 3,
            OpeningBalance = 0,
            StartingPeriod = now,
            ApprenticeshipPaymentSummaries = summaries,
        };

        var sut = new TimelineGenerator(expiredFunds.Object, new CoInvestmentEstimator(new ApprenticeshipOrderingStrategy()));
        
        // act
        var timeline = sut.Create(data);

        // assert
        timeline.Projections[0].CoInvestmentCosts.Should().Be(0m);
        timeline.Projections[1].CoInvestmentCosts.Should().Be(0m);
        timeline.Projections[2].CoInvestmentCosts.Should().Be(0m);
    }
}