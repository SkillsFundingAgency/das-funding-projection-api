using SFA.DAS.EmployerFinance.Types.Models;
using SFA.DAS.FundingProjection.Api.Models.Mappers;
using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Api.Projection;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Mappers;

public class WhenBuildingEstimatesTimelineWithExpiry
{
    private const long AccountId = 12345;
    private static readonly DateOnly StartPeriod = new(2026, 10, 1);

    private static List<CalendarPeriodAmount> GetTwelveMonthsOfLevyIn()
    {
        var results = new List<CalendarPeriodAmount>();
        for (var period = StartPeriod.AddMonths(-12); period < StartPeriod; period = period.AddMonths(1))
        {
            results.Add(new CalendarPeriodAmount { Year = period.Year, Month = period.Month, Amount = period.Month * 100m });
        }
        return results;
    }

    private static CalendarPeriodAmount Amount(DateOnly period, decimal amount) =>
        new() { Year = period.Year, Month = period.Month, Amount = amount };

    private static List<CalendarPeriodAmount> Amounts(int fromMonthOffset, int count, decimal amount) =>
        Enumerable.Range(fromMonthOffset, count).Select(i => Amount(StartPeriod.AddMonths(i), amount)).ToList();

    private static List<LevyInMonthSummary> LevyInEstimates(int months, decimal amount) =>
        Enumerable.Range(0, months).Select(i => new LevyInMonthSummary(StartPeriod.AddMonths(i), amount)).ToList();

    private static List<CommittedLevyProjection> CommitmentEstimates(int months, decimal amount) =>
        Enumerable.Range(0, months).Select(i => new CommittedLevyProjection { Period = StartPeriod.AddMonths(i), TotalMonthlyPayments = amount }).ToList();

    // 24 months of history: 1000 levy in and 400 paid out each month. The first 12 months of levy have
    // already expired at 600 a month (400 short term + 200 long term, to check the two are combined)
    private static EstimatesTimeline BuildTimelineWithAllData(int months) => BuildTimeline(
        months,
        openingBalance: 24 * 1000m - 24 * 400m - 12 * 600m,
        levyFundsIn: Amounts(-24, 24, 1000m),
        paymentFundsOut: Amounts(-24, 24, 400m),
        shortTermExpired: Amounts(-12, 12, 400m),
        longTermExpired: Amounts(-12, 12, 200m),
        levyInProjections: LevyInEstimates(months, 500m),
        commitmentProjections: CommitmentEstimates(months, 300m));

    // mirrors CommitmentsProjector, which always returns an entry for every projected month
    private static List<CommittedLevyProjection> EmptyProjections(int months) =>
        Enumerable.Range(0, months).Select(i => new CommittedLevyProjection { Period = StartPeriod.AddMonths(i) }).ToList();

    private static EstimatesTimeline BuildTimeline(
        int months,
        decimal openingBalance,
        List<CalendarPeriodAmount> levyFundsIn,
        List<CalendarPeriodAmount>? paymentFundsOut = null,
        List<CalendarPeriodAmount>? shortTermExpired = null,
        List<CalendarPeriodAmount>? longTermExpired = null,
        List<LevyInMonthSummary>? levyInProjections = null,
        List<CommittedLevyProjection>? commitmentProjections = null,
        bool currentMonthLevyInReceived = false)
    {
        return EstimatesTimeline.From(
            AccountId,
            StartPeriod,
            months,
            openingBalance,
            0m,
            0m,
            commitmentProjections ?? EmptyProjections(months),
            EmptyProjections(months),
            levyInProjections ?? [],
            new ExpiredFunds(),
            levyFundsIn,
            paymentFundsOut ?? [],
            shortTermExpired ?? [],
            longTermExpired ?? [],
            currentMonthLevyInReceived);
    }

    [Test]
    public void Then_All_Historic_Levy_Expires_Over_12_Months_When_There_Are_No_Estimated_Values()
    {
        // arrange
        var levyFundsIn = GetTwelveMonthsOfLevyIn();
        var totalLevyIn = levyFundsIn.Sum(x => x.Amount);
        var balance = levyFundsIn.Sum(x => x.Amount);

        // act
        var result = BuildTimeline(12, totalLevyIn, levyFundsIn);

        // assert
        var projections = result.Projections;
        projections.Should().HaveCount(12);
        projections.Sum(x => x.LevyOutSummary.ExpiredLevy).Should().Be(totalLevyIn);
        projections[^1].ClosingBalance.Should().Be(0m);
        for (var i = 0; i < 12; i++)
        {
            var levyIn = levyFundsIn[i];
            projections[i].Year.Should().Be(levyIn.Year + 1);
            projections[i].Month.Should().Be(levyIn.Month);
            projections[i].LevyOutSummary.ExpiredLevy.Should().Be(levyIn.Amount);
            balance -= levyFundsIn[i].Amount;
            result.Projections[i].ClosingBalance.Should().Be(balance);
        }
    }

    [Test]
    public void Then_Historic_Payments_Out_Are_Taken_From_The_Oldest_Levy_First()
    {
        // arrange - 1000 levy in for each of the last 12 months, 3000 spent 6 months ago
        var levyFundsIn = Enumerable.Range(-12, 12).Select(i => Amount(StartPeriod.AddMonths(i), 1000m)).ToList();
        var paymentFundsOut = new List<CalendarPeriodAmount> { Amount(StartPeriod.AddMonths(-6), 3000m) };

        // act
        var result = BuildTimeline(12, 9000m, levyFundsIn, paymentFundsOut);

        // assert
        var expired = result.Projections.Select(x => x.LevyOutSummary.ExpiredLevy).ToList();
        expired.Should().Equal(0m, 0m, 0m, 1000m, 1000m, 1000m, 1000m, 1000m, 1000m, 1000m, 1000m, 1000m);
        result.Projections[^1].ClosingBalance.Should().Be(0m);
    }

    [Test]
    public void Then_Projected_Payments_Out_Reduce_The_Levy_That_Expires()
    {
        // arrange - 1000 levy in for each of the last 12 months, 500 committed spend per projected month
        var levyFundsIn = Enumerable.Range(-12, 12).Select(i => Amount(StartPeriod.AddMonths(i), 1000m)).ToList();
        var commitments = Enumerable.Range(0, 12)
            .Select(i => new CommittedLevyProjection { Period = StartPeriod.AddMonths(i), TotalMonthlyPayments = 500m })
            .ToList();

        // act
        var result = BuildTimeline(12, 12000m, levyFundsIn, commitmentProjections: commitments);

        // assert - each month the 500 spent comes from the 1000 about to expire
        result.Projections.Should().AllSatisfy(x => x.LevyOutSummary.ExpiredLevy.Should().Be(500m));
        result.Projections[^1].ClosingBalance.Should().Be(0m);
    }

    [Test]
    public void Then_Already_Expired_Amounts_Are_Used_For_Their_Period()
    {
        // arrange
        var levyFundsIn = GetTwelveMonthsOfLevyIn();
        var shortTermExpired = new List<CalendarPeriodAmount> { Amount(StartPeriod, 250m) };
        var longTermExpired = new List<CalendarPeriodAmount> { Amount(StartPeriod.AddMonths(1), 100m) };

        // act
        var result = BuildTimeline(12, levyFundsIn.Sum(x => x.Amount), levyFundsIn, shortTermExpired: shortTermExpired, longTermExpired: longTermExpired);

        // assert
        result.Projections[0].LevyOutSummary.ExpiredLevy.Should().Be(250m);
        result.Projections[1].LevyOutSummary.ExpiredLevy.Should().Be(100m);
        result.Projections[2].LevyOutSummary.ExpiredLevy.Should().Be(levyFundsIn[2].Amount);
    }

    [Test]
    public void Then_Projected_Levy_In_Expires_12_Months_After_It_Is_Received()
    {
        // arrange
        var levyFundsIn = Enumerable.Range(-12, 12).Select(i => Amount(StartPeriod.AddMonths(i), 1000m)).ToList();
        var levyInProjections = Enumerable.Range(0, 24)
            .Select(i => new LevyInMonthSummary(StartPeriod.AddMonths(i), 400m))
            .ToList();

        // act
        var result = BuildTimeline(24, 12000m, levyFundsIn, levyInProjections: levyInProjections);

        // assert
        var projections = result.Projections;
        projections.Take(12).Should().AllSatisfy(x => x.LevyOutSummary.ExpiredLevy.Should().Be(1000m));
        projections.Skip(12).Should().AllSatisfy(x => x.LevyOutSummary.ExpiredLevy.Should().Be(400m));
        projections[^1].ClosingBalance.Should().Be(12 * 400m);
    }

    [Test]
    public void Then_No_Levy_Expires_When_There_Is_No_Historic_Levy_In()
    {
        // act
        var result = BuildTimeline(6, 0m, []);

        // assert
        result.Projections.Should().HaveCount(6);
        result.Projections.Should().AllSatisfy(x => x.LevyOutSummary.ExpiredLevy.Should().Be(0m));
    }

    [Test]
    public void Then_Expiry_Is_Calculated_From_Existing_Expiry_Levy_Payments_And_Estimates()
    {
        // act
        var result = BuildTimelineWithAllData(24);

        // assert
        // 4800 of the historic spend went on the levy that has already expired, leaving 4800 to spend on the
        // levy that is still available. That plus the 300 a month of estimated spend is taken from the oldest
        // levy first, so nothing expires until month 6. After that only the 300 spent each month is saved, and
        // from month 12 the 500 a month of estimated levy starts to expire
        var expired = result.Projections.Select(x => x.LevyOutSummary.ExpiredLevy).ToList();
        expired.Should().Equal(
            0m, 0m, 0m, 0m, 0m, 0m, 100m, 700m, 700m, 700m, 700m, 700m,
            200m, 200m, 200m, 200m, 200m, 200m, 200m, 200m, 200m, 200m, 200m, 200m);
        result.Projections[^1].ClosingBalance.Should().Be(6000m);
    }

    [Test]
    public void Then_Expiry_Already_Calculated_For_Earlier_Months_Is_Not_Recalculated_As_The_Projection_Extends()
    {
        // act
        var twelveMonths = BuildTimelineWithAllData(12);
        var twentyFourMonths = BuildTimelineWithAllData(24);

        // assert
        twentyFourMonths.Projections.Take(12).Select(x => x.LevyOutSummary.ExpiredLevy)
            .Should().Equal(twelveMonths.Projections.Select(x => x.LevyOutSummary.ExpiredLevy));
    }

    [Test]
    public void Then_Existing_Expiry_Is_Used_Rather_Than_Recalculated()
    {
        // arrange - 24 months of 1000 levy in and 6000 spent 3 months ago
        var levyFundsIn = Amounts(-24, 24, 1000m);
        var paymentFundsOut = new List<CalendarPeriodAmount> { Amount(StartPeriod.AddMonths(-3), 6000m) };
        // recorded expiry says the whole of the first year's levy expired, so none of the 6000 was spent on it
        var existingExpiry = Amounts(-12, 12, 1000m);

        // act
        var withExistingExpiry = BuildTimeline(12, 6000m, levyFundsIn, paymentFundsOut, shortTermExpired: existingExpiry);
        var withoutExistingExpiry = BuildTimeline(12, 6000m, levyFundsIn, paymentFundsOut);

        // assert
        // all 6000 is still available to spend on the second year's levy, so the first 6 months don't expire
        withExistingExpiry.Projections.Select(x => x.LevyOutSummary.ExpiredLevy).Should().Equal(
            0m, 0m, 0m, 0m, 0m, 0m, 1000m, 1000m, 1000m, 1000m, 1000m, 1000m);
        // without it the expiry is recalculated and 3000 of the spend goes on the last 3 months of the first year
        withoutExistingExpiry.Projections.Select(x => x.LevyOutSummary.ExpiredLevy).Should().Equal(
            0m, 0m, 0m, 1000m, 1000m, 1000m, 1000m, 1000m, 1000m, 1000m, 1000m, 1000m);
    }

    [Test]
    public void Then_Levy_Already_Received_This_Month_Is_Shown_But_Not_Added_To_The_Balance()
    {
        // act - the opening balance already includes this month's 500 levy
        var result = BuildTimeline(3, 5000m, [], levyInProjections: LevyInEstimates(3, 500m), currentMonthLevyInReceived: true);

        // assert
        result.Projections[0].LevyInSummary.LevyIn.Should().Be(500m);
        result.Projections[0].ClosingBalance.Should().Be(5000m);
        result.Projections[1].ClosingBalance.Should().Be(5500m);
        result.Projections[2].ClosingBalance.Should().Be(6000m);
    }

    [Test]
    public void Then_Levy_Not_Yet_Received_This_Month_Is_Added_To_The_Balance()
    {
        // act
        var result = BuildTimeline(3, 5000m, [], levyInProjections: LevyInEstimates(3, 500m));

        // assert
        result.Projections[0].LevyInSummary.LevyIn.Should().Be(500m);
        result.Projections[0].ClosingBalance.Should().Be(5500m);
    }
}
