using SFA.DAS.FundingProjection.Api.Projection;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Projection;

public class WhenProjectingLevyIn
{
    private static List<LevyInMonthSummary> GetHistoricData(DateOnly now)
    {
        var results = new List<LevyInMonthSummary>();
        var endPeriod = now.ToPeriod();
        for (var currentPeriod = now.AddYears(-1).ToPeriod(); currentPeriod < endPeriod; currentPeriod = currentPeriod.AddMonths(1))
        {
            results.Add(new LevyInMonthSummary(currentPeriod, currentPeriod.Month * 1000));
        }
        return results;
    }
    
    [Test]
    public void Then_Levy_In_Is_Set_To_Zero_If_There_Is_No_Historic_Data_To_Pull_From()
    {
        // arrange
        var now = DateOnly.FromDateTime(DateTime.UtcNow);

        // act
        var results = LevyInProjector.CreateProjection(now, 3, []);

        // assert
        results.Should().HaveCount(3);
        results.Should().AllSatisfy(x => x.Amount.Should().Be(0));
    }
    
    [Test]
    public void Then_The_Projected_Data_Is_For_The_Period_Specified()
    {
        // arrange
        var now = DateOnly.FromDateTime(DateTime.UtcNow);

        // act
        var results = LevyInProjector.CreateProjection(now, 3, []);

        // assert
        results.Should().HaveCount(3);
        results[0].Period.Should().Be(now.ToPeriod());
        results[1].Period.Should().Be(now.ToPeriod().AddMonths(1));
        results[2].Period.Should().Be(now.ToPeriod().AddMonths(2));
    }
    
    [Test]
    public void Then_The_Historic_Data_Is_Used_To_Project_The_Coming_Levy_In()
    {
        // arrange
        var now = DateOnly.FromDateTime(DateTime.UtcNow).ToPeriod();
        var historicData = GetHistoricData(now);

        // act
        var results = LevyInProjector.CreateProjection(now, 3, historicData);

        // assert
        results.Should().HaveCount(3);
        results[0].Amount.Should().Be(historicData.First(x => x.Period == now.AddYears(-1)).Amount);
        results[1].Amount.Should().Be(historicData.First(x => x.Period == now.AddMonths(1).AddYears(-1)).Amount);
        results[2].Amount.Should().Be(historicData.First(x => x.Period == now.AddMonths(2).AddYears(-1)).Amount);
    }
    
    [Test]
    public void Then_The_Most_Recent_Historic_Data_For_Each_Month_Is_Used()
    {
        // arrange
        var now = DateOnly.FromDateTime(DateTime.UtcNow).ToPeriod();
        List<LevyInMonthSummary> historicData = [..GetHistoricData(now), new (now, 9999m)];

        // act
        var results = LevyInProjector.CreateProjection(now, 3, historicData);

        // assert
        results.Should().HaveCount(3);
        results[0].Amount.Should().Be(9999m);
        results[1].Amount.Should().Be(historicData.First(x => x.Period == now.AddMonths(1).AddYears(-1)).Amount);
        results[2].Amount.Should().Be(historicData.First(x => x.Period == now.AddMonths(2).AddYears(-1)).Amount);
    }
    
    [Test]
    public void Then_If_The_Projection_Period_Extends_Beyond_12_Months_The_Historic_Data_Is_Used_Repeatedly()
    {
        // arrange
        var now = DateOnly.FromDateTime(DateTime.UtcNow).ToPeriod();
        var historicData = GetHistoricData(now);

        // act
        var results = LevyInProjector.CreateProjection(now, 24, historicData);

        // assert
        results.Should().HaveCount(24);
        for (var i = 0; i < 24; i++)
        {
            results[i].Amount.Should().Be(historicData[i % historicData.Count].Amount);
        }
    }
    
    [Test]
    public void Then_If_The_Projection_Period_Extends_Beyond_12_Months_And_The_Historic_Data_Contains_The_Current_Period_It_Is_Used_Correctly_Each_Year()
    {
        // arrange
        var now = DateOnly.FromDateTime(DateTime.UtcNow).ToPeriod();
        List<LevyInMonthSummary> historicData = [..GetHistoricData(now), new (now, 9999m)];

        // act
        var results = LevyInProjector.CreateProjection(now, 24, historicData);

        // assert
        results.Should().HaveCount(24);
        results[0].Amount.Should().Be(9999m);
        results[12].Amount.Should().Be(9999m);
    }
}