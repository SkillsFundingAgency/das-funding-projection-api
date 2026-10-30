using SFA.DAS.EmployerFinance.Types.Models;
using SFA.DAS.FundingProjection.Api.Models.Mappers;
using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Api.Projection;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Mappers;

public class WhenBuildingEstimatesTimelineWithExpiry
{
    private const long AccountId = 12345;
    private static readonly DateOnly StartPeriod = new(2028, 8, 1);
    private static readonly DateOnly BeforePolicyChange = new(2026, 10, 1);

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
    // already expired at 600 a month
    private static EstimatesTimeline BuildTimelineWithAllData(int months) => BuildTimeline(
        months,
        openingBalance: 24 * 1000m - 24 * 400m - 12 * 600m,
        levyFundsIn: Amounts(-24, 24, 1000m),
        paymentFundsOut: Amounts(-24, 24, 400m),
        shortTermExpired: Amounts(-12, 12, 600m),
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
        bool currentMonthLevyInReceived = false,
        DateOnly? startPeriod = null)
    {
        return EstimatesTimeline.From(
            AccountId,
            startPeriod ?? StartPeriod,
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
        var shortTermExpired = new List<CalendarPeriodAmount> { Amount(StartPeriod, 250m), Amount(StartPeriod.AddMonths(1), 100m) };

        // act
        var result = BuildTimeline(12, levyFundsIn.Sum(x => x.Amount), levyFundsIn, shortTermExpired: shortTermExpired);

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

    [Test]
    public void Then_Levy_Received_Before_The_Policy_Change_Expires_After_24_Months()
    {
        // arrange
        var levyFundsIn = new List<CalendarPeriodAmount> { Amount(new DateOnly(2025, 10, 1), 1000m) };

        // act
        var result = BuildTimeline(13, 1000m, levyFundsIn, startPeriod: BeforePolicyChange);

        // assert
        result.Projections[0].LevyOutSummary.ExpiredLevy.Should().Be(0m);
        result.Projections[12].LevyOutSummary.ExpiredLevy.Should().Be(1000m);
    }

    [Test]
    public void Then_Levy_Received_From_The_Policy_Change_Expires_After_12_Months()
    {
        // arrange
        var levyFundsIn = new List<CalendarPeriodAmount> { Amount(new DateOnly(2026, 8, 1), 1000m) };

        // act
        var result = BuildTimeline(12, 1000m, levyFundsIn, startPeriod: BeforePolicyChange);

        // assert - August 2027
        result.Projections[10].LevyOutSummary.ExpiredLevy.Should().Be(1000m);
        result.Projections.Sum(x => x.LevyOutSummary.ExpiredLevy).Should().Be(1000m);
    }

    [Test]
    public void Then_Payments_Out_Are_Taken_From_Long_Term_Levy_Before_Short_Term_Levy()
    {
        // arrange - 1000 of each, and 1000 spent last month
        var levyFundsIn = new List<CalendarPeriodAmount>
        {
            Amount(new DateOnly(2024, 10, 1), 1000m),
            Amount(new DateOnly(2026, 8, 1), 1000m)
        };
        var paymentFundsOut = new List<CalendarPeriodAmount> { Amount(new DateOnly(2026, 9, 1), 1000m) };

        // act
        var result = BuildTimeline(12, 1000m, levyFundsIn, paymentFundsOut, startPeriod: BeforePolicyChange);

        // assert - the spend uses up the long term levy, so only the short term levy expires
        result.Projections[0].LevyOutSummary.ExpiredLevy.Should().Be(0m);
        result.Projections[10].LevyOutSummary.ExpiredLevy.Should().Be(1000m);
        result.Projections.Sum(x => x.LevyOutSummary.ExpiredLevy).Should().Be(1000m);
    }

    [Test]
    public void Then_Long_Term_And_Short_Term_Expiry_For_The_Same_Period_Are_Combined()
    {
        // arrange
        var longTermExpired = new List<CalendarPeriodAmount> { Amount(BeforePolicyChange, 300m) };
        var shortTermExpired = new List<CalendarPeriodAmount> { Amount(BeforePolicyChange, 200m) };

        // act
        var result = BuildTimeline(1, 0m, [], shortTermExpired: shortTermExpired, longTermExpired: longTermExpired, startPeriod: BeforePolicyChange);

        // assert
        result.Projections[0].LevyOutSummary.ExpiredLevy.Should().Be(500m);
    }

    [Test]
    public void Then_Long_Term_And_Short_Term_Levy_Expire_Together_Across_The_Policy_Change()
    {
        // arrange - 1000 a month received from August 2025 and estimated from October 2026, with nothing spent.
        // August 2025 to July 2026 is long term and expires August 2027 to July 2028. August and September 2026
        // are short term and expire August and September 2027, and the estimated levy expires from October 2027
        var levyFundsIn = Enumerable.Range(0, 14).Select(i => Amount(new DateOnly(2025, 8, 1).AddMonths(i), 1000m)).ToList();
        var levyInProjections = Enumerable.Range(0, 24)
            .Select(i => new LevyInMonthSummary(BeforePolicyChange.AddMonths(i), 1000m))
            .ToList();

        // act
        var result = BuildTimeline(24, 14000m, levyFundsIn, levyInProjections: levyInProjections, startPeriod: BeforePolicyChange);

        // assert
        result.Projections.Select(x => x.LevyOutSummary.ExpiredLevy).Should().Equal(
            0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 2000m, 2000m,
            2000m, 2000m, 2000m, 2000m, 2000m, 2000m, 2000m, 2000m, 2000m, 2000m, 1000m, 1000m);
        // only the estimated levy from October 2027 onwards is left
        result.Projections[^1].ClosingBalance.Should().Be(12000m);
    }

    [Test]
    public void Then_Recorded_Expiry_In_One_Pot_Does_Not_Stop_The_Other_Pot_Being_Calculated()
    {
        // arrange - long and short term levy both expire in August 2027, but only the long term has been recorded
        var august2027 = new DateOnly(2027, 8, 1);
        var levyFundsIn = new List<CalendarPeriodAmount>
        {
            Amount(new DateOnly(2025, 8, 1), 1000m),
            Amount(new DateOnly(2026, 8, 1), 1000m)
        };
        var longTermExpired = new List<CalendarPeriodAmount> { Amount(august2027, 600m) };

        // act
        var result = BuildTimeline(1, 2000m, levyFundsIn, longTermExpired: longTermExpired, startPeriod: august2027);

        // assert - the recorded 600 plus the calculated 1000
        result.Projections[0].LevyOutSummary.ExpiredLevy.Should().Be(1600m);
    }

    [Test]
    public void Then_Spend_Less_Than_The_Long_Term_Levy_Leaves_The_Short_Term_Levy_To_Expire_In_Full()
    {
        // arrange
        var levyFundsIn = new List<CalendarPeriodAmount>
        {
            Amount(new DateOnly(2024, 10, 1), 1000m),
            Amount(new DateOnly(2026, 8, 1), 1000m)
        };
        var paymentFundsOut = new List<CalendarPeriodAmount> { Amount(new DateOnly(2026, 9, 1), 500m) };

        // act
        var result = BuildTimeline(12, 1500m, levyFundsIn, paymentFundsOut, startPeriod: BeforePolicyChange);

        // assert - October 2026 (24 months) and August 2027 (12 months)
        result.Projections[0].LevyOutSummary.ExpiredLevy.Should().Be(500m);
        result.Projections[10].LevyOutSummary.ExpiredLevy.Should().Be(1000m);
        result.Projections[^1].ClosingBalance.Should().Be(0m);
    }

    [Test]
    public void Then_Spend_Beyond_The_Long_Term_Levy_Reduces_The_Short_Term_Levy_That_Expires()
    {
        // arrange
        var levyFundsIn = new List<CalendarPeriodAmount>
        {
            Amount(new DateOnly(2024, 10, 1), 1000m),
            Amount(new DateOnly(2026, 8, 1), 1000m)
        };
        var paymentFundsOut = new List<CalendarPeriodAmount> { Amount(new DateOnly(2026, 9, 1), 1500m) };

        // act
        var result = BuildTimeline(12, 500m, levyFundsIn, paymentFundsOut, startPeriod: BeforePolicyChange);

        // assert - 1000 of the spend uses up the long term levy and the other 500 comes from the short term levy
        result.Projections[0].LevyOutSummary.ExpiredLevy.Should().Be(0m);
        result.Projections[10].LevyOutSummary.ExpiredLevy.Should().Be(500m);
        result.Projections[^1].ClosingBalance.Should().Be(0m);
    }
}
