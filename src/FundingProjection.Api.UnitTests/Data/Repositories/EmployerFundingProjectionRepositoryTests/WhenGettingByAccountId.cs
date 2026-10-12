using AutoFixture.NUnit4;
using FluentAssertions;
using Moq;
using SFA.DAS.FundingProjection.Api.UnitTests.Data.DatabaseMock;
using SFA.DAS.FundingProjection.Data;
using SFA.DAS.FundingProjection.Data.Repositories;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Data.Repositories.EmployerFundingProjectionRepositoryTests;

[TestFixture]
internal class WhenGettingByAccountId
{
    // Returns results only for the given employerAccountId
    [Test, RecursiveMoqAutoData]
    public async Task Then_Only_Projections_For_The_Given_EmployerAccountId_Are_Returned(
        long employerAccountId,
        long otherEmployerAccountId,
        CancellationToken token,
        [Frozen] Mock<IFundingProjectionDataContext> context,
        [Greedy] EmployerFundingProjectionRepository repository)
    {
        var now = DateTime.UtcNow;

        var matchingEntities = new List<EmployerFundingProjectionEntity>
    {
        new() { EmployerAccountId = employerAccountId, CalendarPeriodYear = now.Year, CalendarPeriodMonth = now.Month }
    };
        var otherEntities = new List<EmployerFundingProjectionEntity>
    {
        new() { EmployerAccountId = otherEmployerAccountId, CalendarPeriodYear = now.Year, CalendarPeriodMonth = now.Month }
    };

        context.Setup(x => x.EmployerFundingProjections)
            .ReturnsDbSet([.. matchingEntities, .. otherEntities]);

        var actual = await repository.GetTotalCostByMonthsAsync(employerAccountId, 12, token);

        actual.Should().OnlyContain(x => x.EmployerAccountId == employerAccountId);
    }

    // Excludes records older than 12 months
    [Test, RecursiveMoqAutoData]
    public async Task Then_Projections_Older_Than_Twelve_Months_Are_Excluded(
        long employerAccountId,
        CancellationToken token,
        [Frozen] Mock<IFundingProjectionDataContext> context,
        [Greedy] EmployerFundingProjectionRepository repository)
    {
        var now = DateTime.UtcNow;
        var tooOld = new DateTime(now.Year, now.Month, 1).AddMonths(-13);

        var entities = new List<EmployerFundingProjectionEntity>
    {
        new() { EmployerAccountId = employerAccountId, CalendarPeriodYear = tooOld.Year,  CalendarPeriodMonth = tooOld.Month  },
        new() { EmployerAccountId = employerAccountId, CalendarPeriodYear = now.Year,     CalendarPeriodMonth = now.Month     }
    };

        context.Setup(x => x.EmployerFundingProjections)
            .ReturnsDbSet(entities);

        var actual = await repository.GetTotalCostByMonthsAsync(employerAccountId, 12, token);

        actual.Should().ContainSingle()
            .Which.Should().Match<EmployerFundingProjectionEntity>(x =>
                x.CalendarPeriodYear == now.Year && x.CalendarPeriodMonth == now.Month);
    }

    // Excludes future records (beyond the current month)
    [Test, RecursiveMoqAutoData]
    public async Task Then_Future_Projections_Are_Excluded(
        long employerAccountId,
        CancellationToken token,
        [Frozen] Mock<IFundingProjectionDataContext> context,
        [Greedy] EmployerFundingProjectionRepository repository)
    {
        var now = DateTime.UtcNow;
        var future = new DateTime(now.Year, now.Month, 1).AddMonths(1);

        var entities = new List<EmployerFundingProjectionEntity>
    {
        new() { EmployerAccountId = employerAccountId, CalendarPeriodYear = now.Year,    CalendarPeriodMonth = now.Month    },
        new() { EmployerAccountId = employerAccountId, CalendarPeriodYear = future.Year, CalendarPeriodMonth = future.Month }
    };

        context.Setup(x => x.EmployerFundingProjections)
            .ReturnsDbSet(entities);

        var actual = await repository.GetTotalCostByMonthsAsync(employerAccountId, 12, token);

        actual.Should().ContainSingle()
            .Which.Should().Match<EmployerFundingProjectionEntity>(x =>
                x.CalendarPeriodYear == now.Year && x.CalendarPeriodMonth == now.Month);
    }

    // Includes the boundary month exactly 12 months ago
    [Test, RecursiveMoqAutoData]
    public async Task Then_Projection_Exactly_Twelve_Months_Ago_Is_Included(
        long employerAccountId,
        CancellationToken token,
        [Frozen] Mock<IFundingProjectionDataContext> context,
        [Greedy] EmployerFundingProjectionRepository repository)
    {
        var now = DateTime.UtcNow;
        var boundary = new DateTime(now.Year, now.Month, 1).AddMonths(-12);

        var entities = new List<EmployerFundingProjectionEntity>
    {
        new() { EmployerAccountId = employerAccountId, CalendarPeriodYear = boundary.Year, CalendarPeriodMonth = boundary.Month }
    };

        context.Setup(x => x.EmployerFundingProjections)
            .ReturnsDbSet(entities);

        var actual = await repository.GetTotalCostByMonthsAsync(employerAccountId, 12, token);

        actual.Should().ContainSingle()
            .Which.Should().Match<EmployerFundingProjectionEntity>(x =>
                x.CalendarPeriodYear == boundary.Year && x.CalendarPeriodMonth == boundary.Month);
    }

    // Returns empty list when no projections exist
    [Test, RecursiveMoqAutoData]
    public async Task Then_Empty_List_Is_Returned_When_No_Projections_Exist(
        long employerAccountId,
        CancellationToken token,
        [Frozen] Mock<IFundingProjectionDataContext> context,
        [Greedy] EmployerFundingProjectionRepository repository)
    {
        context.Setup(x => x.EmployerFundingProjections)
            .ReturnsDbSet(new List<EmployerFundingProjectionEntity>());

        var actual = await repository.GetTotalCostByMonthsAsync(employerAccountId, 12, token);

        actual.Should().BeEmpty();
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_The_ApplicationReviews_Not_Matched_Then_No_Results_Returned(
        long employerAccountId,
        int months,
        CancellationToken token,
        List<EmployerFundingProjectionEntity> employerFundingProjectionEntities,
        [Frozen] Mock<IFundingProjectionDataContext> context,
        [Greedy] EmployerFundingProjectionRepository repository)
    {
        context.Setup(x => x.EmployerFundingProjections)
            .ReturnsDbSet(employerFundingProjectionEntities);

        var actual = await repository.GetTotalCostByMonthsAsync(employerAccountId, months, token);

        actual.Should().BeEmpty();
    }
}