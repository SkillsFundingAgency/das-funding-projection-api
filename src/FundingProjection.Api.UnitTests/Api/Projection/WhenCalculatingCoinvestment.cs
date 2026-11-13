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
        timeline.Projections[0].DfeCoInvestmentCost.Should().Be(75m);
        timeline.Projections[1].DfeCoInvestmentCost.Should().Be(75m);
        timeline.Projections[2].DfeCoInvestmentCost.Should().Be(131.25m);
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
        timeline.Projections[0].DfeCoInvestmentCost.Should().Be(100m);
        timeline.Projections[1].DfeCoInvestmentCost.Should().Be(100m);
        timeline.Projections[2].DfeCoInvestmentCost.Should().Be(175m);
    }

    [Test, MoqAutoData]
    public void Then_The_Levy_Pays_What_It_Can_And_The_Shortfall_Is_Split_So_The_Month_Reconciles(
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
                Dob = now.AddMonths(-1).AddYears(-20),
            }
        ];

        var data = new TimelineData
        {
            AccountId = accountId,
            Months = 1,
            OpeningBalance = 40m,
            StartingPeriod = now,
            ApprenticeshipPaymentSummaries = summaries,
        };

        var sut = new TimelineGenerator(expiredFunds.Object, new CoInvestmentEstimator(new ApprenticeshipOrderingStrategy()));

        // act
        var timeline = sut.Create(data);

        // assert
        var month = timeline.Projections[0];
        month.ClosingBalance.Should().Be(0m);
        month.CoInvestmentCosts.Should().Be(0m);
        month.DfeCoInvestmentCost.Should().Be(60m);
        (40m + month.CoInvestmentCosts + month.DfeCoInvestmentCost)
            .Should().Be(month.LevyOutSummary.Total - month.LevyOutSummary.ExpiredLevy);
    }

    [Test, MoqAutoData]
    public void Then_The_Actual_CoInvestment_Payments_Are_Used_If_They_Have_Been_Made(
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
                Dob = now.AddMonths(-1).AddYears(-25),
            }
        ];

        var data = new TimelineData
        {
            AccountId = accountId,
            Months = 3,
            OpeningBalance = 0,
            StartingPeriod = now,
            ApprenticeshipPaymentSummaries = summaries,
            CurrentPeriodPayments = new CurrentPeriodPayments { CoInvestment = 12m },
        };

        var sut = new TimelineGenerator(expiredFunds.Object, new CoInvestmentEstimator(new ApprenticeshipOrderingStrategy()));

        // act
        var timeline = sut.Create(data);

        // assert
        timeline.Projections[0].CoInvestmentCosts.Should().Be(12m);
        timeline.Projections[1].CoInvestmentCosts.Should().Be(25m);
        timeline.Projections[2].CoInvestmentCosts.Should().Be(43.75m);
    }
}
